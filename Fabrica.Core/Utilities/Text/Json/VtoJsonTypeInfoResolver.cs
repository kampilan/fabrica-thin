using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Fabrica.Persistence.Entities;

namespace Fabrica.Utilities.Text.Json;

public class VtoJsonTypeInfoResolver: CompactJsonTypeInfoResolver
{

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {


        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;

        if( !type.IsAssignableTo(typeof(IEntity)) )
            return typeInfo;


        foreach (var prop in typeInfo.Properties)
        {
            if( prop.Name == "Id" || prop.Name == "id" || prop.Name.EndsWith("Id") )

                prop.ShouldSerialize = (_, _) => false;
        }

        return typeInfo;


    }

}