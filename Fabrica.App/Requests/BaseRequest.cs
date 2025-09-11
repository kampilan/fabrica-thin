using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Models;
using MediatR;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Requests;

public abstract class AbstractRequest
{

    [JsonExtensionData]
    [JsonInclude]
    protected Dictionary<string, JsonElement> Overposts { get; set; } = new();

    public bool IsOverposted() => Overposts.Count > 0;

    public bool IsNotOverposted() => Overposts.Count == 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;
    public string GetOverpostMessage() => $"These properties do not exist or are immutable: ({string.Join(',', Overposts.Keys)})";
   

}

public abstract class BaseRequest : AbstractRequest, IRequest<Response>
{
    public string Uid { get; set; } = string.Empty;
}

