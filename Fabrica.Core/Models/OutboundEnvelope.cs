using System.Text.Json.Serialization;
using Fabrica.Exceptions;

namespace Fabrica.Models;

public class OutboundEnvelope
{

    public static OutboundEnvelope FromResponse(IResponse response)
    {

        var env = new OutboundEnvelope
        {
            CorrelationId = response.CorrelationId,
        };

        var info   = response.Details.Where(d => d.Category == EventDetail.EventCategory.Info).Select(d => new EnvelopeEvent {Kind = EnvelopeEventKind.Info,  Message = d.Explanation});
        var alerts = response.Details.Where(d => d.Category == EventDetail.EventCategory.Warning).Select(d => new EnvelopeEvent { Kind = EnvelopeEventKind.Alert,  Message = d.Explanation });

        env.Events.AddRange(info);
        env.Events.AddRange(alerts);

        return env;

    }

    public static OutboundEnvelope FromResponse(IValueResponse response)
    {

        var env = new OutboundEnvelope
        {
            CorrelationId = response.CorrelationId,
            Content       = response.Value
        };

        var info = response.Details.Where(d => d.Category == EventDetail.EventCategory.Info).Select(d => new EnvelopeEvent { Kind = EnvelopeEventKind.Info, Message = d.Explanation });
        var alerts = response.Details.Where(d => d.Category == EventDetail.EventCategory.Warning).Select(d => new EnvelopeEvent { Kind = EnvelopeEventKind.Alert, Message = d.Explanation });

        env.Events.AddRange(info);
        env.Events.AddRange(alerts);

        return env;

    }




    public string CorrelationId { get; set; } = string.Empty;

    public object? Content { get; set; }

    public List<EnvelopeEvent> Events { get; set; } = new();

    public Dictionary<string,object> Attributes { get; set; } = new();

}

public enum EnvelopeEventKind
{
    Info,
    Alert
}

public class EnvelopeEvent
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EnvelopeEventKind Kind { get; set; } = EnvelopeEventKind.Info;

    public string Message { get; set; } = string.Empty;

}