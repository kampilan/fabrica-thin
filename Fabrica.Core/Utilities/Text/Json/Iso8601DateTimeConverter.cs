using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Utilities.Text.Json;

public class Iso8601DateTimeConverter : JsonConverter<DateTime>
{
    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime().ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var dt = value.ToUniversalTime();
        var iso = dt.ToString("yyyy-MM-ddTHH:mm:ssK");
        writer.WriteStringValue(iso);
    }

}