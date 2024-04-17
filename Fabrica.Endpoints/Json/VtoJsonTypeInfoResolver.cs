using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using Fabrica.Persistence;

namespace Fabrica.Json;

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
            if( prop.Name == "Id" || prop.Name.EndsWith("Id") )

                prop.ShouldSerialize = (_, _) => false;
        }

        return typeInfo;


    }

}