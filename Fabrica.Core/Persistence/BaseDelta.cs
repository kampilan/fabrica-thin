using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable CollectionNeverUpdated.Local

namespace Fabrica.Persistence;

public class BaseDelta
{

    [JsonExtensionData]
    [JsonInclude]
    protected Dictionary<string,JsonElement> Overposts { get; set; } = new ();

    public bool IsOverposted() => Overposts.Count > 0;

    public bool IsNotOverposted() => Overposts.Count == 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;
    public string GetOverpostMessage() => $"These properties do not exist or are immutable: ({string.Join(',', Overposts.Keys)})" ;


}
