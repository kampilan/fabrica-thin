using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fabrica.App.Persistence.Converters;
    

public class JsonNodeConvertor() : ValueConverter<JsonObject, string>( node => FromObject(node), str => ToObject(str) )
{

    private static readonly JsonSerializerOptions Options = new () { WriteIndented = false };
    
    static string FromObject(JsonObject jo )
    {
        var json = jo.ToJsonString(Options);
        return json;

    }
    
    static JsonObject ToObject(string json)
    {
        var jn = JsonNode.Parse(json);
        if( jn is JsonObject jo )
            return jo;

        return new JsonObject();

    }
    
    
}


