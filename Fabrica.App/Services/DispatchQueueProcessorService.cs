using Amazon.SQS;
using Autofac;
using CommunityToolkit.Diagnostics;
using Fabrica.App.Requests;
using Fabrica.Watch;
using IRequestMediator = Fabrica.App.Mediator.IRequestMediator;

namespace Fabrica.App.Services;

public abstract class DispatchQueueProcessorService<TService,TMessage>(IAmazonSQS sqs, string queueName, ILifetimeScope rootScope): AbstractQueueProcessorService<TService,TMessage>(sqs, queueName, rootScope) where TService : AbstractQueueProcessorService<TService,TMessage> where TMessage : class
{
    
    protected abstract Task<BaseRequest> BuildRequest( QueueMessage<TMessage> message, ILogger logger );
    
    protected sealed override async Task<ProcessResult> ProcessMessageAsync( QueueMessage<TMessage> message, ILogger logger, ILifetimeScope scope, CancellationToken ct )
    {
        
        Guard.IsNotNull(message);       
        Guard.IsNotNull(logger);
        Guard.IsNotNull(scope);
        
        
        // *************************************************
        logger.Debug("Attempting to build Request");
        BaseRequest request;
        try
        {
            request = await BuildRequest(message, logger);
        }
        catch (Exception cause)
        {
            logger.Error(cause, "Failed to build Request");           
            return ProcessResult.FailedPermanent;       
        }


        
        // *************************************************
        logger.Debug("Attempting to send Request through the Mediator");
        var mediator = scope.Resolve<IRequestMediator>();
        var response = await mediator.Send( request, ct );

        if( response.IsSuccessful ) 
            return ProcessResult.Completed;


        
        // *************************************************        
        logger.Error($"Dispatch failed - {response.Explanation}");
        return ProcessResult.FailedPermanent;

        
    }

    
}