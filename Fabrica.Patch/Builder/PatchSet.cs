using System.Reflection;
using System.Text.Json;
using Fabrica.Patch.Models;
using Fabrica.Patch.Resolver;
using Fabrica.Watch;


// ReSharper disable UnusedMember.Global

namespace Fabrica.Patch.Builder
{


    public class PatchSet
    {


        public static PatchSet FromJsonOne(string json)
        {

            var patch = JsonSerializer.Deserialize<ModelPatch>(json, Options);

            var set = new PatchSet();
            set.Patches.Add(patch!);

            return set;

        }

        public static PatchSet FromJsonOne(Stream stream)
        {


            var patch = JsonSerializer.Deserialize<ModelPatch>(stream, Options);

            var set = new PatchSet();
            set.Patches.Add(patch!);

            return set;


        }


        public static PatchSet FromJsonMany(string json)
        {

            var set = new PatchSet
            {
                Patches = JsonSerializer.Deserialize<List<ModelPatch>>(json, Options)!
            };

            return set;

        }

        public static PatchSet FromJsonMany(Stream stream)
        {

            var set = new PatchSet
            {
                Patches = JsonSerializer.Deserialize<List<ModelPatch>>(stream, Options)!
            };

            return set;


        }


        public static PatchSet Create(IMutableModel model)
        {

            var set = new PatchSet();
            set.Add(model);

            return set;

        }

        public static PatchSet Create(IEnumerable<IMutableModel> models)
        {

            var set = new PatchSet();
            set.Add(models);

            return set;

        }

        public static PatchSet Create(params IMutableModel[] models)
        {

            var set = new PatchSet();
            set.Add(models);

            return set;

        }




        public ModelPatch<TModel> CreatePatch<TModel>(string uid, PatchVerb verb = PatchVerb.Update) where TModel : class
        {

            var mp = new ModelPatch<TModel>(uid, verb);
            Add(mp);

            return mp;

        }

        public void Add(IMutableModel model)
        {
            HandleObject("", "", "", model);

            After();

        }

        public void Add(params IMutableModel[] models)
        {

            foreach (var m in models)
                HandleObject("", "", "", m);

            After();

        }

        public void Add(IEnumerable<IMutableModel> models)
        {

            foreach (var m in models)
                HandleObject("", "", "", m);

            After();

        }

        public void Add(PatchSet set)
        {
            if (!set.Equals(this))
                Patches.AddRange(set.Patches);
        }

        public void Add(IEnumerable<ModelPatch> patches)
        {
            Patches.AddRange(patches);
        }

        public void Add(params ModelPatch[] patches)
        {
            Patches.AddRange(patches);
        }


        private void After()
        {

            bool Rules(ModelPatch p)
            {

                if (p.Verb == PatchVerb.Unmodified)
                    return false;

                if (p is { Verb: PatchVerb.Update, Properties.Count: 0 })
                    return false;

                if (p is { Verb: PatchVerb.Create, Properties.Count: 0 })
                    return false;

                return true;

            }

            Patches = Patches.Where(Rules).ToList();

        }


        private List<ModelPatch> Patches { get; set; } = new();

        public int Count => Patches.Count;

        public bool HasModifications => Patches.Count > 0;

        public IEnumerable<ModelPatch> GetPatches()
        {

            bool Rules(ModelPatch p)
            {

                if (p.Verb == PatchVerb.Unmodified)
                    return false;

                if (p is { Verb: PatchVerb.Update, Properties.Count: 0 })
                    return false;

                if (p is { Verb: PatchVerb.Create, Properties.Count: 0 })
                    return false;

                return true;

            }

            var patches = Patches.Where(Rules);

            return patches;

        }


        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };


        public string ToJson()
        {
            var json = JsonSerializer.Serialize(Patches, Options);
            return json;
        }


        protected virtual void HandleObject(string parentType, string parentUid, string parentProperty, IMutableModel source)
        {

            if (parentType == null) throw new ArgumentNullException(nameof(parentType));
            if (parentUid == null) throw new ArgumentNullException(nameof(parentUid));
            if (parentProperty == null) throw new ArgumentNullException(nameof(parentProperty));
            if (source == null) throw new ArgumentNullException(nameof(source));


            using var logger = this.EnterMethod();


            logger.Inspect(nameof(parentType), parentType);
            logger.Inspect(nameof(parentUid), parentUid);
            logger.Inspect(nameof(parentProperty), parentProperty);

            logger.Inspect("Model Type", source.GetType().FullName);
            logger.Inspect("Model Target", source.GetType().Name);
            logger.Inspect("Model IsModified", source.IsModified());
            logger.Inspect("Model IsAdded", source.IsAdded());
            logger.Inspect("Model IsRemoved", source.IsRemoved());


            var state = PatchVerb.Unmodified;
            if (source.IsAdded())
                state = PatchVerb.Create;
            else if (source.IsRemoved())
                state = PatchVerb.Delete;
            else if (source.IsModified())
                state = PatchVerb.Update;

            var name = source.GetType().Name;
            var attr = source.GetType().GetCustomAttribute<ResolveAttribute>();
            if( attr is not null && !string.IsNullOrWhiteSpace(attr.Alias))
                name = attr.Alias;

            var patch = new ModelPatch
            {
                Model = name,
                Uid = source.GetUid(),
                Verb = state,
            };

            if (!string.IsNullOrWhiteSpace(parentType))
            {
                var ap = new PropertyPath
                {
                    Model = parentType,
                    Uid = parentUid,
                    Property = parentProperty
                };

                patch.Membership = ap;
            }



            // *****************************************************************
            logger.Debug("Attempting to add Patch to PatchSet");
            Patches.Add(patch);



            // *****************************************************************
            if (source.IsRemoved())
            {
                logger.Debug("Model being removed. No need to look for modification");
                return;
            }



            // *****************************************************************
            logger.Debug("Attempting to look for modified properties");
            foreach (var prop in source.GetDelta().Values)
            {

                if (prop is { IsReference: true, Current: null, Original: IReferenceModel })
                    patch.Properties[prop.Name] = "";
                else if (prop is { IsReference: true, Current: IReferenceModel cur })
                    patch.Properties[prop.Name] = cur.GetUid();
                else if (prop is { IsCollection: true, Current: IDependentCollection collection })
                    HandleCollection(prop.Parent, prop.ParentUid, prop.Name, collection);
                else
                    patch.Properties[prop.Name] = prop.Current!;

            }

            logger.LogObject(nameof(patch), patch);



        }

        protected virtual void HandleCollection(string parentType, string parentUid, string parentProperty, IDependentCollection set)
        {


            if (set == null) throw new ArgumentNullException(nameof(set));
            if (string.IsNullOrWhiteSpace(parentType))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentType));
            if (string.IsNullOrWhiteSpace(parentUid))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentUid));
            if (string.IsNullOrWhiteSpace(parentProperty))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentProperty));


            foreach (var agg in set.DeltaMembers.Cast<IDependentModel>())
            {
                if (agg.IsModified() && !agg.IsAdded() && !agg.IsRemoved())
                    HandleObject("", "", "", agg);
                else
                    HandleObject(parentType, parentUid, parentProperty, agg);
            }


        }



    }

}
