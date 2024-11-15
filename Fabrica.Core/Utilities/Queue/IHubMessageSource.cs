namespace Fabrica.Utilities.Queue;

public interface IHubMessageSource
{
    Task<(bool,HubQueueMessage?, ICompletionHandle? handle)> Get( CancellationToken token = default  );

    Task Complete(ICompletionHandle handle, CancellationToken token = default);

}