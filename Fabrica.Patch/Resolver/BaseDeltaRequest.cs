using System.Reflection;
using Fabrica.Patch.Builder;

namespace Fabrica.Patch.Resolver;

public class BaseDeltaRequest: BaseEntityRequest, IPatchRequest
{

    public string Uid { get; set; } = "";

    public Dictionary<string, object> Delta { get; set; } = new();

    public void FromObject( object source )
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        foreach (var pi in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
        {
            var value = pi.GetValue(source, null);
            if (value is not null)
                Delta[pi.Name] = value;
        }

    }

    public void FromPatch( ModelPatch patch )
    {

        if (patch == null) throw new ArgumentNullException(nameof(patch));

        if( patch.Verb == PatchVerb.Update )
            Uid = patch.Uid;

        Delta = new Dictionary<string, object>(patch.Properties);

    }




}