using Amazon.SQS;
using Fabrica.Utilities.Queue;

namespace Fabrica.Aws;

internal class SqsHubQueue( IAmazonSQS client, string queueName, string signingKey, int timeout = 20, int count = 1) : BaseSqsQueueManager(client,queueName), IHubMessageSink, IHubMessageSource
{

    public async Task Send( HubQueueMessage message, CancellationToken token=default )
    {

        var (body,signature) = message.Save(signingKey);

        await HandleSend( signature, body, token );

    }

    public async Task<(bool,HubQueueMessage?, ICompletionHandle? )> Get( CancellationToken token = default  )
    {
        
        var result = await HandleReceive( timeout, count, token );

        if( result == EmptyReceive )
            return (false, null, null);

        var message = HubQueueMessage.Load( result.Body, signingKey, result.Signature );


        var handle = new CompletionHandle { QueueUrl = result.QueueUrl, Receipt = result.ReceiptHandle };

        return (true, message, handle);


    }

}