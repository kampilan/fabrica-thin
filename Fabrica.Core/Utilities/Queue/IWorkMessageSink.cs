namespace Fabrica.Utilities.Queue;

public interface IWorkMessageSink
{
    Task Send( WorkQueueMessage message, CancellationToken token = default);

}