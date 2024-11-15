using Amazon.SQS;
using Amazon.SQS.Model;
using Fabrica.Utilities.Queue;
using Fabrica.Watch;

namespace Fabrica.Aws;

internal abstract class BaseSqsQueueManager(IAmazonSQS client, string queueName)
{

    protected record ReceiveResult(string Signature, string Body, string QueueUrl, string ReceiptHandle);
 
    protected static ReceiveResult EmptyReceive = new ("", "", "", "");


    protected async Task HandleSend(string body, CancellationToken token = default)
    {

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(body));

        using var logger = this.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to get queue url");

        var queueUrlRes = await client.GetQueueUrlAsync(queueName, token);
        var queueUrl = queueUrlRes.QueueUrl;

        logger.Inspect(nameof(queueUrl), queueUrl);



        // *****************************************************************
        logger.Debug("Attempting to build SQS send Request");
        var sqsReq = new SendMessageRequest(queueUrl, body);



        // *****************************************************************
        logger.Debug("Attempting to Send request to SQS");
        var sqsRes = await client.SendMessageAsync(sqsReq, token);

        logger.LogObject(nameof(sqsRes), sqsRes);


    }


    protected async Task HandleSend( string signature, string body, CancellationToken token = default )
    {

        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(signature));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(body));


        using var logger = this.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to get queue url");

        var queueUrlRes = await client.GetQueueUrlAsync(queueName, token);
        var queueUrl = queueUrlRes.QueueUrl;

        logger.Inspect(nameof(queueUrl), queueUrl);



        // *****************************************************************
        logger.Debug("Attempting to build SQS send Request");
        var sqsReq = new SendMessageRequest(queueUrl, body);
        sqsReq.MessageAttributes.Add("Signature", new MessageAttributeValue {DataType = "String", StringValue = signature});



        // *****************************************************************
        logger.Debug("Attempting to Send request to SQS");
        var sqsRes = await client.SendMessageAsync(sqsReq, token);

        logger.LogObject(nameof(sqsRes), sqsRes);


    }


    protected async Task<ReceiveResult> HandleReceive( int timeout, int count, CancellationToken token = default )
    {

        var queueUrlRes = await client.GetQueueUrlAsync(queueName, token);
        var queueUrl = queueUrlRes.QueueUrl;

        var req = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MessageAttributeNames = ["Signature"],
            WaitTimeSeconds = timeout,
            MaxNumberOfMessages = count
        };


        var res = await client.ReceiveMessageAsync(req, token);


        if( res.Messages.Count == 0 )
            return EmptyReceive;


        var received = res.Messages[0];

        var signature = received.MessageAttributes["Signature"]?.StringValue??"";
        var body      = received.Body;
        var receipt   = received.ReceiptHandle;


        return new ReceiveResult( signature, body, queueUrl, receipt );

    }

    public async Task Complete( ICompletionHandle handle, CancellationToken token=default )
    {

        using var logger = this.EnterMethod();

        if (handle is not CompletionHandle impl)
            return;

        logger.Debug("Attempting to delete Message");
        var ackReq = new DeleteMessageRequest
        {
            QueueUrl = impl.QueueUrl,
            ReceiptHandle = impl.Receipt
        };

        var ackRes = await client.DeleteMessageAsync( ackReq, token );

        logger.LogObject(nameof(ackRes), ackRes);


    }

}