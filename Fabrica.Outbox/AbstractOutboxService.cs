using System.Data;
using System.Data.Common;
using Dapper;
using Fabrica.Persistence.Outbox;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.Outbox;

[UsedImplicitly]
public abstract class AbstractOutboxService<TOutbox>(IOutboxSignal signal): BackgroundService where TOutbox: class, IOutbox
{

    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    protected abstract DbConnection GetConnection();
    protected abstract string GetFetchSql();
    protected abstract string GetCompletionSql();

    protected abstract Task ProcessOutboxAsync( TOutbox outbox );

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        using var logger = this.EnterMethod();
        
        while( !stoppingToken.IsCancellationRequested )
        {

            
            // *************************************************
            logger.Debug("Attempting to wait for outbox signal");
            if( !signal.Wait( TimeSpan.FromSeconds(2) ) )
                continue;


            
            // *************************************************
            logger.Debug("Got outbox signal");
            
            TOutbox? current = null;
            try
            {

                
                // *************************************************
                logger.Debug("Attempting to get DbConnection");
                await using var connection = GetConnection();

                if( connection.State == ConnectionState.Closed )
                    await connection.OpenAsync(stoppingToken);

                while( !stoppingToken.IsCancellationRequested )
                {

                    
                    // *************************************************
                    logger.Debug("Attempting to fetch Outbox entity");
                    await using var transaction = await connection.BeginTransactionAsync(stoppingToken);

                    var fetchSql = GetFetchSql();
                    logger.LogSql("Fetch", fetchSql);

                    current = await connection.QueryFirstOrDefaultAsync<TOutbox>(fetchSql, transaction: transaction);

                    if( current is not null )
                    {

                        logger.Debug("Found Outbox entity. Processing it.");
                        
                        await ProcessOutboxAsync( current );


                        
                        var completionSql = GetCompletionSql();
                        logger.LogSql("Completion", completionSql);                        

                        var affected = await connection.ExecuteAsync(completionSql, new { current.Id }, transaction: transaction);

                        logger.Inspect(nameof(affected), affected);
                        

                        
                        await transaction.CommitAsync(stoppingToken);

                    }
                    else
                    {

                        logger.Debug("Did NOT find Outbox entity.");                        
                        
                        await transaction.CommitAsync(stoppingToken);
                        break;
                        
                    }
                    
                }

                current = null;
                
                await Task.Delay(Interval, stoppingToken);
                
            }
            catch (OperationCanceledException)
            {
                // This is expected when the service is shutting down.
            }
            catch (Exception ex)
            {

                if( current is not null )
                {
                    var ctx = new { current.Id, current.Description  };
                    logger.ErrorWithContext(ex, ctx, "Unexpected error processing outbox.");
                }
                else
                    logger.ErrorWithContext(ex, "Unexpected error processing outbox.");                

                await Task.Delay(Interval, stoppingToken);
                
            }
            
        }


    }
    
}