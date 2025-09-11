
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Fabrica.App.Services;

/// <summary>
/// Represents a message to be processed by a queueing system.
/// </summary>
/// <typeparam name="T">
/// The type of the message body, constrained to reference types.
/// </typeparam>
public class QueueMessage<T> where T : class
{

    public string Topic { get; set; } = string.Empty; 

    public Dictionary<string,string> Attributes { get; set; } = new ();
    
    public T Body { get; set; } = null!;
    
}