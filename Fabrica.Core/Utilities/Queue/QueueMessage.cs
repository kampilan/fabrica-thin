
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fabrica.Utilities.Queue;

public interface IQueueMessage
{
    string Topic { get; }
    Dictionary<string,string> Attributes { get; }
    object Body { get; }
}    

/// <summary>
/// Represents a message to be processed by a queueing system.
/// </summary>
/// <typeparam name="T">
/// The type of the message body, constrained to reference types.
/// </typeparam>
public class QueueMessage<T>: IQueueMessage where T : class
{

    public string Topic { get; set; } = string.Empty; 

    public Dictionary<string,string> Attributes { get; set; } = [];
    
    public T Body { get; set; } = null!;

    object IQueueMessage.Body => Body; 
    
}


public class JsonQueueMessage : QueueMessage<JsonNode>
{

    public T? To<T>() where T : class
    {

        var json = Body.ToJsonString();
        var obj = JsonSerializer.Deserialize<T>(json);
        return obj;
        
    }
    
    
}
