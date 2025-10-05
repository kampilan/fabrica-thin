using System.Text.Json.Nodes;

namespace Fabrica.Persistence.Outbox;

public interface IOutbox
{

    long Id { get;  }
    string Description { get; }
    string Destination { get; }
    string Topic { get; }
    JsonObject Payload { get; }
    
}