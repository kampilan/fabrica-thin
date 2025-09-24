using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Autofac;
using CommunityToolkit.Diagnostics;
using Fabrica.App.Requests;
using Fabrica.Watch;

namespace Fabrica.App.Services;

public abstract class S3EventProcessorService(IAmazonSQS sqs, string queueName, IAmazonS3 s3, ILifetimeScope rootScope) : DispatchQueueProcessorService<S3EventProcessorService, S3Event>(sqs, queueName, rootScope)
{

    protected override async Task<BaseRequest> BuildRequest( S3Event message, ILogger logger )
    {

        Guard.IsNotNull(message);
        Guard.IsNotNull(logger);
        
        if( message.Records is null || message.Records.Count == 0 )
            throw new Exception("No records in message");

        var record = message.Records[0];
        
        if( record.S3 is null )
            throw new Exception("No S3 details in record");

        if(string.IsNullOrWhiteSpace(record.EventName))
            throw new Exception("No EventName in record");

        var eventName = record.EventName;

        if(string.IsNullOrWhiteSpace(record.EventTime))
            throw new Exception("No EventTime in record");
        
        var occurred = DateTime.Parse(record.EventTime);

        
        if(string.IsNullOrWhiteSpace(record.EventSource))
            throw new Exception("No EventSource in record");
        
        var eventSource = record.EventSource;
        
        var s3o = record.S3;

        if( s3o.Object is null )
            throw new Exception("No Object details in S3");

        if( s3o.Bucket is null )
            throw new Exception("No Bucket details in S3");

        var bucket = s3o.Bucket;
        if(string.IsNullOrWhiteSpace(bucket.Name))
            throw new Exception("No Bucket Name in Bucket");

        var bucketName = bucket.Name;
        
        
        var obj = s3o.Object;        
        if(string.IsNullOrWhiteSpace(obj.Key))
            throw new Exception("No Key in Object");
        
        var key = obj.Key;
        
        // *************************************************
        logger.Debug("Attempting to get pre-signed url for the object");
        var s3Req = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.Now.AddMinutes(60),
        };

        var url = await s3.GetPreSignedURLAsync(s3Req);

        
        
        // *************************************************
        logger.Debug("Attempting to build request");
        var request = new S3EventRequest
        {
            Bucket = bucketName,       
            Key = key,
            Operation = eventName,
            Occurred = occurred,
            Size = obj.Size,
            SourceUrl = url
        };

        
        // *************************************************        
        return request;

    }
    
    
}

public class S3EventRequest : BaseRequest
{

    public string Bucket { get; set; } = string.Empty;    
    public string Key { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime Occurred { get; set; } = DateTime.MinValue;
    public long Size { get; set; } = 0;
    
    public string SourceUrl { get; set; } = string.Empty;
    
}




public class S3Event
{

    public List<S3EventRecord> Records { get; set; } = new();

}


public class S3EventRecord
{

    public string EventVersion { get; set; } = "";

    public string EventSource { get; set; } = "";

    public string AwsRegion { get; set; } = "";

    public string EventTime { get; set; } = "";

    public string EventName { get; set; } = "";

    public S3EventS3 S3 { get; set; } = null!;

}

public class S3EventS3
{
 
    public S3EventBucket Bucket { get; set; } = null!;

    public S3EventObject Object { get; set; } = null!;

}

public class S3EventBucket
{

    public string Name { get; set; } = "";

    public string Arn { get; set; } = "";

}


public class S3EventObject
{

    public string Key { get; set; } = "";

    public long Size { get; set; }

    public string ETag { get; set; } = "";

    public string VersionId { get; set; } = "";

    public string Sequencer { get; set; } = "";

}




