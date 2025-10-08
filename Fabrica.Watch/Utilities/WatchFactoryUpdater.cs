
// ReSharper disable MemberCanBePrivate.Global

using JetBrains.Annotations;

namespace Fabrica.Watch.Utilities;

public class WatchFactoryUpdater
{

    public TimeSpan EventFlushInterval { get; set; } = TimeSpan.FromSeconds(10);    
    public TimeSpan SwitchUpdateInterval { get; set; } = TimeSpan.FromSeconds(60);

    private System.Timers.Timer? _flushTimer;
    private System.Timers.Timer? _updateTimer;    
    private CancellationTokenSource? _cts;
    
    
    [UsedImplicitly]
    public void Start()
    {

        _cts = new CancellationTokenSource();
        
        _flushTimer = new System.Timers.Timer( 100 );
        _flushTimer.AutoReset = true;
        _flushTimer.Elapsed += async (_,_) =>
        {
            _flushTimer.Enabled = false;
            await WatchFactoryLocator.Factory.FlushEventsAsync( EventFlushInterval, _cts.Token );
            _flushTimer.Enabled = true;
        };
        _flushTimer.Start();

        
        _updateTimer = new System.Timers.Timer( SwitchUpdateInterval.TotalMilliseconds );
        _updateTimer.AutoReset = true;
        _updateTimer.Elapsed += async (_,_) =>
        {
            _updateTimer.Enabled = false;
            await WatchFactoryLocator.Factory.UpdateSwitchesAsync( _cts.Token );
            _updateTimer.Enabled = true;
        };
        _updateTimer.Start();        
        
        
    }

    [UsedImplicitly]
    public void Stop()
    {

        _flushTimer?.Stop();
        _flushTimer?.Dispose();
        _flushTimer = null;       

        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        _updateTimer = null;      
        
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;       
        
    }    
    
}