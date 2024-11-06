namespace Fabrica.Models;

public class Envelope<T> where T: class
{

    public string CorrelationId { get; set; } = string.Empty;

    public T? Content { get; set; }

    public List<EnvelopeEvent> Events { get; set; } = new();

    public Dictionary<string, object> Attributes { get; set; } = new();


}

public class Envelope
{
    public string CorrelationId { get; set; } = string.Empty;

    public List<EnvelopeEvent> Events { get; set; } = new();

    public Dictionary<string, object> Attributes { get; set; } = new();
}