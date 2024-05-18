using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Fabrica.Patch.Builder;

public class ModelPatch
{

    [DefaultValue(PatchVerb.Update)]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PatchVerb Verb { get; set; } = PatchVerb.Update;
    public string Model { get; set; } = "";
    public string Uid { get; set; } = "";


    [JsonIgnore]
    public bool IsMember => Membership != null;

    [DefaultValue(null)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PropertyPath? Membership { get; set; }


    public IDictionary<string,object> Properties { get; set; } = new Dictionary<string,object>();


}


public class ModelPatch<TModel> : ModelPatch where TModel: class
{

    public ModelPatch( string uid, PatchVerb verb=PatchVerb.Update )
    {

        Model = typeof(TModel).Name;
        Uid   = uid;
        Verb  = verb;

    }


    public ModelPatch<TModel> Set<TProp>( Expression<Func<TModel,TProp>> prop, TProp value )
    {

        if( prop.Body is MemberExpression {NodeType: ExpressionType.MemberAccess} me ) 
        {
            Properties.Add( me.Member.Name, value! );
        }

        return this;

    }

}