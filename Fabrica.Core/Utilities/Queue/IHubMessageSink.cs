namespace Fabrica.Utilities.Queue;

public interface IHubMessageSink
{
    Task Send(HubQueueMessage message, CancellationToken token = default);

}