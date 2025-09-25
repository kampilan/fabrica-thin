namespace Fabrica.Persistence.Outbox;

public interface IOutbox
{

    long Id { get;  }
    string Description { get; }
    string Destination { get; }
    string Topic { get; }
    string Payload { get; }
    
}