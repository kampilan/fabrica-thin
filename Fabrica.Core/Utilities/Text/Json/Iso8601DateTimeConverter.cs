using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Utilities.Text.Json;

public class Iso8601DateTimeConverter : JsonConverter<DateTime>
{
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var iso = value.ToString("yyyy-MM-ddTHH:mm:ssZ");
        writer.WriteStringValue(iso);
    }

}