namespace Fabrica.Utilities.Queue;

public interface IWorkMessageSource
{

    Task<(bool,WorkQueueMessage?, ICompletionHandle? handle)> Get( CancellationToken token = default  );

    Task Complete( ICompletionHandle handle, CancellationToken token = default);

}