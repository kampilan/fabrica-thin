using CommunityToolkit.Diagnostics;
using Fabrica.App.Persistence.Contexts;
using Fabrica.Persistence.Outbox;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable UnusedType.Global

namespace Fabrica.App.Persistence.Interceptors;

public class OutboxInterceptor<TOutbox>(ICorrelation correlation, IOutboxSignal signal) : SaveChangesInterceptor where TOutbox : class, IOutbox
{

    
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken)
    {

        Guard.IsNotNull(eventData, nameof(eventData));
        
        using var logger = correlation.EnterMethod<OutboxInterceptor<TOutbox>>();

        
        // *************************************************
        logger.Debug("Attempting to check if any entities where saved");
        logger.Inspect(nameof(eventData.EntitiesSavedCount), eventData.EntitiesSavedCount);
        if( eventData.EntitiesSavedCount == 0 )
            return ValueTask.FromResult(result);


        
        // *************************************************        
        logger.Inspect(nameof(eventData.Context), eventData.Context is not null );
        
        var hasOutboxEntities = false;
        if( eventData.Context is not null )
            hasOutboxEntities = eventData.Context.ChangeTracker.Entries<TOutbox>().Any();

        logger.Inspect( "HasOutboxEntities", hasOutboxEntities );
        
        if( !hasOutboxEntities )
            return ValueTask.FromResult(result);    
        
        
        
        // *************************************************
        logger.Debug("Attempting to check if and Outbox entities were saved");
        if( eventData is { EntitiesSavedCount: > 0, Context: not null } && eventData.Context.ChangeTracker.Entries<TOutbox>().Any() )
        {
            logger.Debug("Found Outbox instances, signaling");
            signal.Set();            
        }

        
        // *************************************************        
        return ValueTask.FromResult(result);
        
    }
    
}