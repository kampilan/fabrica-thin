using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.SQS;
using Amazon.SQS.Model;
using Fabrica.Persistence.Outbox;
using Fabrica.Utilities.Queue;
using Fabrica.Watch;

namespace Fabrica.Outbox;

public abstract class SqsOutboxService<TOutbox>( IOutboxSignal signal, IAmazonSQS sqs ) : AbstractOutboxService<TOutbox>(signal) where TOutbox: class, IOutbox
{

    protected override async Task ProcessOutboxAsync(TOutbox outbox)
    {

        using var logger = this.EnterMethod();
        

        
         // *************************************************
        logger.Debug("Attempting to parse outbox payload");
        logger.LogJson( nameof(outbox.Payload), outbox.Payload );
        JsonNode? body = null;
        try
        {
            body = JsonNode.Parse(outbox.Payload);
        }
        catch (Exception cause)
        {
            var ctx = new { outbox.Id, outbox.Description, outbox.Destination, outbox.Topic };
            logger.ErrorWithContext( cause, ctx, "Failed to process outbox. Could not parse outbox payload" );
        }
        
        if( body is null )
            return;
        

        
         // *************************************************
        logger.Debug("Attempting to build and serialize message");
        string json;
        try
        {

            var message = new JsonQueueMessage
            {
                Topic = outbox.Topic,
                Body = body,
            };
            
            json = JsonSerializer.Serialize(message);
            
        }
        catch (Exception cause)
        {
            var ctx = new { outbox.Id, outbox.Description, outbox.Destination, outbox.Topic };
            logger.ErrorWithContext( cause, ctx, "Failed to process outbox. Could not serialize message" );
            throw;                        
        }


        
         // *************************************************
        logger.Debug("Attempting to get SQS Queue URL");        
        string url;
        try
        {
            var res = await sqs.GetQueueUrlAsync(outbox.Destination);
            url = res.QueueUrl;
        }
        catch (Exception cause)
        {
            var ctx = new { outbox.Id, outbox.Description, outbox.Destination, outbox.Topic };
            logger.ErrorWithContext( cause, ctx, "Failed to process outbox. Could not get SQS Url" );
            throw;
        }



        // *************************************************
        logger.Debug("Attempting to send message via SQS");
        try
        {

            var request = new SendMessageRequest
            {
                QueueUrl = url,
                MessageBody = json
            };

            await sqs.SendMessageAsync(request);
            
        }
        catch (Exception cause)
        {
            var ctx = new { outbox.Id, outbox.Description, outbox.Destination, outbox.Topic };
            logger.ErrorWithContext( cause, ctx, "Failed to process outbox. Could not get SQS Url" );
            throw;
        }


    }
    
    
}