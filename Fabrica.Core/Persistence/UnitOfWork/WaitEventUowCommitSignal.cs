
// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.Persistence.UnitOfWork;

public sealed class WaitEventUowCommitSignal: IUnitOfWorkCommitSignal
{

    public TimeSpan SignalInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    
    private readonly ManualResetEvent _signal = new (false);

    public void Set()
    {

        _signal.Set();
        
        Task.Run( async () =>
        {
            await Task.Delay(SignalInterval);
            _signal.Reset();
        });
        
    }
    
    public bool Wait( TimeSpan timeout=default )
    {
        
        var signaled = _signal.WaitOne(timeout);
        
        return signaled;
        
    }

    
}