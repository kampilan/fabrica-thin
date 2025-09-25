using Fabrica.Persistence.Outbox;

namespace Fabrica.Outbox;

public class OutboxSignal: IOutboxSignal
{

    private readonly AutoResetEvent _signal = new (false);

    public void Set()
    {
        _signal.Set();
    }
    
    public bool Wait(TimeSpan timeout=default)
    {

        if( timeout == TimeSpan.Zero )
            timeout = TimeSpan.FromSeconds(10);
        
        var signaled = _signal.WaitOne(timeout);
        
        return signaled;
        
    }
    
}