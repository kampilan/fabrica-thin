using System.Collections.Immutable;
using System.Reflection;
using Fabrica.Patch.Builder;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Patch.Resolver;

public class ResolverService( TypeSource source ) : IRequiresStart
{

    private ImmutableDictionary<string, ResolverEntry> _resolvers = null!;

    public Task Start()
    {

        using var logger = this.EnterMethod();


        var entries = new Dictionary<string, ResolverEntry>();


        // *****************************************************************
        logger.Debug("Attempting to scan given assemblies");
        foreach (var type in source.GetTypes())
        {

            var attr = type.GetCustomAttribute<ResolveAttribute>();
            if (attr is null)
                continue;


            var alias = attr.Alias;
            if ( string.IsNullOrWhiteSpace(alias) )
                alias = attr.Target.Name;


            var entry = new ResolverEntry
            {
                Target    = attr.Target,
                Operation = attr.Operation,
                Request   = type
            };


            entries.Add($"{alias}-{attr.Operation}", entry);

        }

        logger.LogObject(nameof(entries), entries);


        _resolvers = ImmutableDictionary.CreateRange(entries);


        return Task.CompletedTask;

    }


    public List<IPatchRequest> GetRequests( PatchSet set )
    {


        using var logger = this.EnterMethod();


        var list = new List<IPatchRequest>();

        foreach (var patch in set.GetPatches())
        {

            switch (patch.Verb)
            {
                case PatchVerb.Create when patch.Membership is not null:

                    if (!_resolvers.ContainsKey($"{patch.Model}-{ResolveOperation.CreateMember}"))
                        throw new InvalidOperationException($"No resolver found for {patch.Model}-{ResolveOperation.CreateMember}");

                    var me = _resolvers[$"{patch.Model}-{ResolveOperation.CreateMember}"];

                    var mr = Build(patch, me);

                    list.Add(mr);

                    break;

                case PatchVerb.Create when patch.Membership is null:

                    if (!_resolvers.ContainsKey($"{patch.Model}-{ResolveOperation.Create}"))
                        throw new InvalidOperationException($"No resolver found for {patch.Model}-{ResolveOperation.Create}");

                    var ce = _resolvers[$"{patch.Model}-{ResolveOperation.Create}"];

                    var cr = Build(patch, ce);

                    list.Add(cr);

                    break;

                case PatchVerb.Update:

                    if (!_resolvers.ContainsKey($"{patch.Model}-{ResolveOperation.Update}"))
                        throw new InvalidOperationException($"No resolver found for {patch.Model}-{ResolveOperation.Update}");

                    var ue = _resolvers[$"{patch.Model}-{ResolveOperation.Update}"];

                    var ur = Build(patch, ue);

                    list.Add(ur);

                    break;

                case PatchVerb.Delete:

                    if (!_resolvers.ContainsKey($"{patch.Model}-{ResolveOperation.Delete}"))
                        throw new InvalidOperationException($"No resolver found for {patch.Model}-{ResolveOperation.Delete}");

                    var de = _resolvers[$"{patch.Model}-{ResolveOperation.Delete}"];

                    var dr = Build(patch, de);

                    list.Add(dr);

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        return list;


        IPatchRequest Build(ModelPatch patch, ResolverEntry entry)
        {

            var obj = Activator.CreateInstance(entry.Request)!;
            if (obj is IPatchRequest pr)
            {
                pr.FromPatch(patch);
                return pr;
            }

            throw new InvalidOperationException($"{entry.Request.FullName} is not IPatchRequest");


        }
        

    }



}

public class ResolverEntry
{

    public Type Target { get; set; } = null!;
    public ResolveOperation Operation { get; set; }

    public Type Request { get; set; } = null!;


}