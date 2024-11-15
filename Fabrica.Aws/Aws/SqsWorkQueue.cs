using Amazon.SQS;
using Fabrica.Utilities.Queue;

namespace Fabrica.Aws;

internal class SqsWorkQueue(IAmazonSQS client, string queueName, string signingKey, int timeout = 20, int count = 1) : BaseSqsQueueManager(client, queueName), IWorkMessageSink, IWorkMessageSource
{

    public async Task Send(WorkQueueMessage message, CancellationToken token = default)
    {

        var (body, signature) = message.Save(signingKey);

        await HandleSend(signature, body, token);

    }

    public async Task<(bool, WorkQueueMessage?, ICompletionHandle?)> Get(CancellationToken token = default)
    {

        var result = await HandleReceive(timeout, count, token);

        if (result == EmptyReceive)
            return (false, null, null);

        var message = WorkQueueMessage.Load(result.Body, signingKey, result.Signature);

        var handle = new CompletionHandle {QueueUrl = result.QueueUrl, Receipt = result.ReceiptHandle};

        return (true, message, handle);

    }

}