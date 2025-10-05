using System.Data;
using System.Data.Common;
using System.Text.Json.Nodes;
using Dapper;
using Fabrica.Persistence.Outbox;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.Outbox;

[UsedImplicitly]
public abstract class AbstractOutboxService<TOutbox>(IUnitOfWorkCommitSignal signal): BackgroundService where TOutbox: class, IOutbox
{

    static AbstractOutboxService()
    {
        SqlMapper.AddTypeHandler(new JsonObjectHandler());
    }

    private class JsonObjectHandler : SqlMapper.TypeHandler<JsonObject>
    {
        public override void SetValue(IDbDataParameter parameter, JsonObject? value)
        {
            parameter.Value = value?.ToJsonString()??"{}";       
        }

        public override JsonObject? Parse(object value)
        {
            if( value is string json )
                return JsonNode.Parse(json)?.AsObject()??new JsonObject();
            else
                return new JsonObject();
        }
        
    }    
    
    
    public TimeSpan PauseAfterErrorInterval { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan PauseAfterNotFoundInterval { get; set; } = TimeSpan.FromMilliseconds(25);
    
    protected abstract DbConnection GetConnection();
    protected abstract string GetFetchSql();
    protected abstract string GetCompletionSql();

    protected abstract Task ProcessOutboxAsync( TOutbox outbox );

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var correlation = new Correlation();
        var firstTime = true;
        
        while( !stoppingToken.IsCancellationRequested )
        {
            
            TOutbox? current = null;
            try
            {

                if (!firstTime && !signal.Wait(TimeSpan.FromSeconds(2)))
                    continue;

                firstTime = false;


                correlation = new Correlation();
                using var logger = correlation.EnterMethod(GetType());


                // *************************************************
                logger.Debug("Attempting to get DbConnection");
                await using var connection = GetConnection();

                if( connection.State == ConnectionState.Closed )
                    await connection.OpenAsync(stoppingToken);

                
                while( !stoppingToken.IsCancellationRequested )
                {

                    
                    
                    // *************************************************
                    logger.Debug("Attempting to begin transaction");
                    await using var transaction = await connection.BeginTransactionAsync(stoppingToken);


                    
                    // *************************************************
                    logger.Debug("Attempting to fetch Outbox entity");
                    var fetchSql = GetFetchSql();
                    logger.LogSql("Fetch", fetchSql);
                    
                    current = await connection.QueryFirstOrDefaultAsync<TOutbox>(fetchSql, transaction: transaction);

                    if( current is not null )
                    {

                        
                        // *************************************************                        
                        logger.Debug("Found Outbox entity. Processing it.");
                        await ProcessOutboxAsync( current );


                        
                        // *************************************************
                        logger.Debug("Attempting to complete Outbox entity");                        
                        var completionSql = GetCompletionSql();
                        logger.LogSql("Completion", completionSql);                        

                        var affected = await connection.ExecuteAsync(completionSql, new { current.Id }, transaction: transaction);

                        logger.Inspect(nameof(affected), affected);


                        
                        // *************************************************
                        logger.Debug("Attempting to commit transaction");                        
                        await transaction.CommitAsync(stoppingToken);

                    }
                    else
                    {

                        
                        // *************************************************                        
                        logger.Debug("Did NOT find Outbox entity.");                        


                        
                        // *************************************************
                        logger.Debug("Attempting to commit transaction");                        
                        await transaction.CommitAsync(stoppingToken);

                        
                        // *************************************************                        
                        await Task.Delay(PauseAfterNotFoundInterval, stoppingToken);
                        
                        
                        // *************************************************                        
                        break;
                        
                    }
                    
                }

                current = null;
                
                
            }
            catch (OperationCanceledException)
            {
                // This is expected when the service is shutting down.
            }
            catch (Exception ex)
            {

                using var logger = correlation.GetLogger(GetType());                
                
                if( current is not null )
                {

                    var ctx = new { current.Id, current.Description  };
                    logger.ErrorWithContext(ex, ctx, "Unexpected error processing outbox.");
                }
                else
                    logger.ErrorWithContext(ex, "Unexpected error processing outbox.");                

                await Task.Delay(PauseAfterErrorInterval, stoppingToken);
                
            }
            
        }


    }
    
}