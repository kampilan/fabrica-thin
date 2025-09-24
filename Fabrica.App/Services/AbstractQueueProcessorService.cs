using System.Diagnostics;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Autofac;
using CommunityToolkit.Diagnostics;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.Extensions.Hosting;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Services;


/// <summary>
/// Represents the result of a processing operation. This enumeration is used to indicate whether
/// the processing of a message or task was successful, encountered a recoverable failure,
/// or resulted in a non-recoverable permanent failure.
/// </summary>
public enum ProcessResult { FailedTransient, FailedPermanent, Completed }

/// <summary>
/// Represents an abstract base class designed to process messages from an Amazon SQS queue. This class is intended to be inherited to implement
/// specific queue-processing services.
/// </summary>
/// <typeparam name="TService">
/// The type of the inheriting service class, which must inherit from AbstractQueueProcessorService itself.
/// </typeparam>
/// <typeparam name="TMessage">
/// The type of the messages being processed by the service. This type must be a reference type.
/// </typeparam>
/// <remarks>
/// AbstractQueueProcessorService provides a framework for handling and processing messages retrieved from an SQS queue.
/// It includes queue validation, message deserialization, and message processing capabilities, which can be customized by overriding specific methods.
/// </remarks>
/// <example>
/// This class is meant to be subclassed, as seen in the examples like DispatchQueueProcessorService and GoogleUpdateService. Inherited services
/// are expected to define how to process messages by implementing the <see cref="ProcessMessageAsync"/> method.
/// </example>
public abstract class AbstractQueueProcessorService<TService,TMessage>( IAmazonSQS sqs, string queueName, ILifetimeScope rootScope ): BackgroundService where TService : AbstractQueueProcessorService<TService,TMessage> where TMessage : class
{

    private readonly string _serviceName = typeof(TService).GetConciseName();
    
    protected JsonSerializerOptions JsonBodyOptions { get; set; } = JsonSerializerOptions.Default;
  
    protected virtual async Task StartupAsync( CancellationToken ct )
    {

        var correlation = new Correlation();        
        using var logger = correlation.EnterMethod<TService>();

        logger.InfoFormat("QueueProcessingService: {0} starting", _serviceName);

        logger.Debug("Attempting to Validate configuration");
        try
        {
            try
            {
                logger.EnterScope($"{typeof(TService).GetConciseFullName()}.{nameof(ValidateConfiguration)}");
                await ValidateConfiguration(logger, ct);
            }
            finally
            {
                logger.LeaveScope($"{typeof(TService).GetConciseFullName()}.{nameof(ValidateConfiguration)}");
            }
        }
        catch (Exception cause)
        {
            logger.Error(cause, "QueueProcessingService: {0} failed validation. It will NOT start.", _serviceName);
            return;
        }

        logger.InfoFormat("QueueProcessingService: {0} started", _serviceName);
        
    }

    
    /// <summary>
    /// Validates the configuration settings required for the queue processor service to function properly.
    /// Ensures that necessary dependencies, including the SQS client setup, are correctly configured.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected virtual async Task ValidateConfiguration( ILogger logger, CancellationToken ct )
    {

        
        // *************************************************
        logger.Debug("Attempting to validate QueueName");
        logger.Inspect(nameof(queueName), queueName);
        if (string.IsNullOrWhiteSpace(queueName))
            throw new Exception("SqsClient is not configured properly. QueueName is blank");

        logger.Inspect(nameof(queueName), queueName);
        

        // *************************************************
        logger.Debug("Attempting to validate QueueUrl");
        var queueUrl = await GetQueueUrl(ct);
        if (string.IsNullOrWhiteSpace(queueUrl))
            throw new Exception("SqsClient is not configured properly. Queue URL is blank");

        logger.Inspect(nameof(queueUrl), queueUrl);

        
        
    }

    
    protected virtual TMessage ParseMessage( string message, ILogger logger )
    {
        
        // *************************************************
        logger.Debug("Attempting to parse body into QueueMessage");

        var qm = JsonSerializer.Deserialize<TMessage>(message, JsonBodyOptions );

        if (qm is null)
            throw new Exception("Parse produced null QueueMessage");

        logger.LogObject(nameof(qm), qm);

        return qm;
        
    }


    /// <summary>
    /// Processes a message asynchronously within the context of a predefined lifecycle scope.
    /// Responsible for handling the logic associated with processing a specific queue message and managing the outcome.
    /// </summary>
    /// <param name="message">The queue message to be processed, encapsulated in a <see cref="QueueMessage{TMessage}"/>.</param>
    /// <param name="logger">The logger instance used for logging actions during the processing of the message.</param>
    /// <param name="scope">The lifetime scope instance used to manage scoped dependencies during the message processing.</param>
    /// <param name="ct">A cancellation token that propagates notifications if the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation. The result is a <see cref="ProcessResult"/> indicating the outcome of the processing.</returns>
    protected abstract Task<ProcessResult> ProcessMessageAsync( TMessage message, ILogger logger, ILifetimeScope scope, CancellationToken ct);


    /// <summary>
    /// Executes the main processing loop for the queue processor service.
    /// Continuously retrieves messages from the configured queue and processes them until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that signals when the process should stop.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ********************************************************************************        
        await StartupAsync(stoppingToken);


        // ********************************************************************************        
        while( !stoppingToken.IsCancellationRequested )
        {

            var body = "";
            var step = "Top";
            try
            {

                // ***********************************************************************
                step = "Receive Message";
                var receiveResult = await ReceiveMessage( TimeSpan.FromSeconds(20), stoppingToken );
                if( receiveResult is not { received: true, msg: not null } )
                    continue;


                // ***********************************************************************                
                step = "Handle Message";
                body = receiveResult.msg.Body;

                await using var scope = rootScope.BeginLifetimeScope();
                var correlation = scope.Resolve<ICorrelation>();

                using var innerLogger = correlation.GetLogger<TService>();
                var category = $"{typeof(TService).GetConciseFullName()}.{nameof(ProcessMessageAsync)}";
                innerLogger.EnterScope(category);

                    
                // ***********************************************************************                
                try
                {

                    var start = Stopwatch.GetTimestamp();

                    
                    // ***********************************************************************                    
                    step = "Parse Message";
                    var message = ParseMessage(body, innerLogger);

                    
                    // ***********************************************************************                    
                    step = "Process Message";
                    var procResult = await ProcessMessageAsync(message, innerLogger, scope, stoppingToken);

                    var elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - start);

                    if( innerLogger.IsDebugEnabled )
                        innerLogger.Debug($"Processed message on Service ({typeof(TService).GetConciseName()}) in {elapsed.TotalMicroseconds} microseconds");


                    
                    // ***********************************************************************                
                    step = "Delete Message";
                    if( procResult is ProcessResult.Completed or ProcessResult.FailedPermanent )
                        await DeleteMessage(receiveResult.msg, stoppingToken);

                    
                }
                catch (Exception cause)
                {
                    var ctx = new { Service=_serviceName, queueName, Step=step, BodyLength = body.Length };
                    innerLogger.ErrorWithContext( cause, ctx, $"Failed to process message at step: {step} on Service: ({_serviceName}) from Queue: ({queueName})." );
                }
                finally
                {
                    innerLogger.LeaveScope(category);
                }
            }
            catch (TaskCanceledException)
            {
                using var logger = WatchFactoryLocator.Factory.GetLogger<TService>();               
                logger.Debug($"ReceiveMessage cancelled for Service: ({GetType().GetConciseName()}) Shutting down? {stoppingToken.IsCancellationRequested}");                
            }            
            catch (Exception cause)
            {
                using var logger = WatchFactoryLocator.Factory.GetLogger<TService>();
                var ctx = new { Service=_serviceName, queueName, Step=step, BodyLength = body.Length };
                logger.ErrorWithContext( cause, ctx, $"Failed to process message at step: {step} on Service: ({_serviceName}) from Queue: ({queueName})." );
                await Task.Delay(1000, stoppingToken);
            }
            
        }        
        
    }


    /// <summary>
    /// Retrieves the URL of the specified SQS queue based on its name.
    /// This method communicates with the SQS service to resolve the queue's URL.
    /// </summary>
    /// <param name="ct">A cancellation token used to cancel the asynchronous operation, if needed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the URL of the queue as a string.</returns>
    private async Task<string> GetQueueUrl( CancellationToken ct=default )
    {

        var response = await sqs.GetQueueUrlAsync(queueName, ct);
        var queueUrl = response.QueueUrl;
        
        // *************************************************        
        return queueUrl;
        
    }


    /// <summary>
    /// Receives a message from the configured SQS queue, waiting for a specified duration.
    /// Retrieves at most one message from the queue and handles exceptions that may occur
    /// during the operation. If no messages are retrieved within the specified wait time,
    /// the method returns indicating no message was received.
    /// </summary>
    /// <param name="waitTime">The amount of time to wait for a message from the queue before timing out.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. On completion, returns a tuple containing
    /// a boolean indicating whether a message was received, and the received message object if one was retrieved.
    /// </returns>
    private async Task<(bool received, Message? msg)> ReceiveMessage( TimeSpan waitTime, CancellationToken ct=default )
    {


        var queueUrl = await GetQueueUrl(ct);

        
        try
        {

            var req = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = Convert.ToInt32(waitTime.TotalSeconds)
            };

            var res = await sqs.ReceiveMessageAsync(req, ct);
            if (res?.Messages is null || res.Messages.Count == 0)
                return (false, null);

            return (true, res.Messages[0]);

        }
        catch( TaskCanceledException )
        {
            throw;            
        }
        catch (Exception cause)
        {
            using var logger = WatchFactoryLocator.Factory.GetLogger<TService>();
            logger.Error(cause, $"Failed to receive message on Queue: ({queueName}) for Service: ({_serviceName})");
            throw;
        }        

        
    }


    /// <summary>
    /// Deletes a message from the configured SQS queue using the provided message details.
    /// Ensures that the message is removed from the queue upon successful execution.
    /// </summary>
    /// <param name="msg">The SQS message to be deleted. This parameter cannot be null.</param>
    /// <param name="ct">The CancellationToken that can be used to observe cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation of deleting the message.</returns>
    private async Task DeleteMessage(Message msg, CancellationToken ct=default)
    {

        Guard.IsNotNull(msg);
        
        var queueUrl = await GetQueueUrl(ct);

        try
        {
            var request = new DeleteMessageRequest( queueUrl, msg.ReceiptHandle );
            await sqs.DeleteMessageAsync(request, ct);
        }
        catch (Exception cause)
        {
            using var logger = WatchFactoryLocator.Factory.GetLogger<TService>();
            logger.Error(cause, $"Failed to delete message on Queue: ({queueName})");
            throw;
        }
        
    }
    
    
}