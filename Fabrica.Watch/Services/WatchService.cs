using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.Watch.Services;

[UsedImplicitly]
public class WatchService: BackgroundService
{

    public TimeSpan SwitchUpdateInterval { get; set; } = TimeSpan.FromSeconds(15);
    
    private ConsoleEventSink DebugSink { get; } = new ();
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        await base.StartAsync(cancellationToken);


        // *******************************************************        
        try
        {
            await WatchFactoryLocator.Factory.Switches.UpdateAsync(cancellationToken);
        }
        catch (Exception cause)
        {
            using var logger = DebugSink.GetLogger<WatchService>();
            logger.Error(cause, "Failed to Update Switches");    
        }

        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while( !stoppingToken.IsCancellationRequested)
        {

            
            // *******************************************************            
            try
            {
                await WatchFactoryLocator.Factory.FlushEventsAsync( SwitchUpdateInterval, stoppingToken );
            }
            catch (Exception cause)
            {
                using var logger = DebugSink.GetLogger<WatchService>();               
                logger.Error(cause, "Failed to Flush WatchFactory");                
            }


            
            // *******************************************************            
            try
            {
                // ReSharper disable once PossiblyMistakenUseOfCancellationToken
                await WatchFactoryLocator.Factory.Switches.UpdateAsync(stoppingToken);
            }
            catch (Exception cause)
            {
                using var logger = DebugSink.GetLogger<WatchService>();               
                logger.Error(cause, "Failed to Update Switches");                
            }            
            
        }
        
    }
    
    
}