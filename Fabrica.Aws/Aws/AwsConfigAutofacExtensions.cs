using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SQS;
using Autofac;
using CommunityToolkit.Diagnostics;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Queue;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Aws;


public static class AwsConfigAutofacExtensions
{

    public class MetaDataDefaults
    {

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);
        public string InstanceId { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string UserData { get; set; } = string.Empty;
        
    }
    
    
    public static ContainerBuilder UseAws(this ContainerBuilder builder, string profileName="", Action<MetaDataDefaults>? defaults = null )
    {

        var md = new MetaDataDefaults();

        defaults?.Invoke(md);

        
        builder.Register(_ =>
            {

                var comp = new InstanceMetaService
                {
                    DefaultInstanceId = md.InstanceId,
                    DefaultRegion     = md.Region,
                    DefaultUserData   = md.UserData,
                    Timeout           = md.Timeout
                };
                
                return comp;
                
            })
            .As<IInstanceMetadata>()
            .As<IRequiresStart>()
            .SingleInstance();
        
        
        if (string.IsNullOrWhiteSpace(profileName))
            return builder;

        var sharedFile = new SharedCredentialsFile();
        if (!(sharedFile.TryGetProfile(profileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials)))
            throw new Exception($"Local profile {profile} could not be loaded");


        AWSConfigs.AWSRegion = profile.Region.SystemName;


        builder.Register(_ => credentials)
            .As<AWSCredentials>()
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddS3Client(this ContainerBuilder builder, string regionName)
    {
        
        builder.Register(c =>
            {

                RegionEndpoint? region = null;
                if( !string.IsNullOrWhiteSpace(regionName) )
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null && region is not null)
                    return new AmazonS3Client(credentials, region);

                if (credentials is not null)
                    return new AmazonS3Client(credentials);

                if (region is not null)
                    return new AmazonS3Client(region);

                return new AmazonS3Client();


            })
            .As<IAmazonS3>()
            .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddS3Client(this ContainerBuilder builder )
    {

        builder.Register(c =>
        {

            var credentials = c.ResolveOptional<AWSCredentials>();

            if( credentials is not null )
                return new AmazonS3Client(credentials);
           
            
            return new AmazonS3Client();


        })
        .As<IAmazonS3>()
        .SingleInstance();


        return builder;

    }



    public static ContainerBuilder AddSqsClient(this ContainerBuilder builder, string regionName)
    {

        builder.Register(c =>
            {

                RegionEndpoint? region = null;
                if (!string.IsNullOrWhiteSpace(regionName))
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null && region is not null)
                    return new AmazonSQSClient(credentials, region);

                if (credentials is not null)
                    return new AmazonSQSClient(credentials);

                if (region is not null)
                    return new AmazonSQSClient(region);

                return new AmazonSQSClient();


            })
            .As<IAmazonSQS>()
            .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddSqsClient(this ContainerBuilder builder)
    {

        builder.Register(c =>
            {

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null)
                    return new AmazonSQSClient(credentials);


                return new AmazonSQSClient();


            })
            .As<IAmazonSQS>()
            .SingleInstance();


        return builder;

    }


    public static ContainerBuilder AddHubMessageQueue(this ContainerBuilder builder, string queueName, string signingKey)
    {

        builder.Register(c =>
            {
                var client = c.Resolve<IAmazonSQS>();

                return new SqsHubQueue(client, queueName, signingKey);

            })
            .As<IHubMessageSink>()
            .As<IHubMessageSource>()
            .InstancePerDependency();


        return builder;

    }

    public static ContainerBuilder AddWorkMessageQueue(this ContainerBuilder builder, string queueName, string signingKey)
    {

        builder.Register(c =>
            {
                var client = c.Resolve<IAmazonSQS>();

                return new SqsWorkQueue(client, queueName, signingKey);

            })
            .As<IWorkMessageSink>()
            .As<IWorkMessageSource>()
            .InstancePerDependency();


        return builder;

    }


    public static ContainerBuilder AddStsClient(this ContainerBuilder builder, string regionName)
    {
        
        
        builder.Register(c =>
            {

                RegionEndpoint? region = null;
                if (!string.IsNullOrWhiteSpace(regionName))
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null && region is not null)
                    return new AmazonSecurityTokenServiceClient(credentials, region);

                if (region is not null)
                    return new AmazonSecurityTokenServiceClient(region);

                return new AmazonSecurityTokenServiceClient();


            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }

    public static ContainerBuilder AddStsClient(this ContainerBuilder builder, string accessKey, string secretKey, string regionName )
    {

        Guard.IsNotNullOrWhiteSpace(accessKey, nameof(accessKey));
        Guard.IsNotNullOrWhiteSpace(secretKey, nameof(secretKey));
        
        builder.Register(_ =>
            {

                RegionEndpoint? region = null;
                if (!string.IsNullOrWhiteSpace(regionName))
                    region = RegionEndpoint.GetBySystemName(regionName);

                var credentials = new BasicAWSCredentials(accessKey, secretKey);

                if (region is not null)
                    return new AmazonSecurityTokenServiceClient(credentials, region);

                return new AmazonSecurityTokenServiceClient(credentials);


            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }    

    public static ContainerBuilder AddStsClient(this ContainerBuilder builder)
    {

        builder.Register(c =>
            {

                var credentials = c.ResolveOptional<AWSCredentials>();

                if (credentials is not null)
                    return new AmazonSecurityTokenServiceClient(credentials);


                return new AmazonSecurityTokenServiceClient();


            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }

    public static ContainerBuilder AddStsClient(this ContainerBuilder builder, string accessKey, string secretKey )
    {

        Guard.IsNotNullOrWhiteSpace(accessKey, nameof(accessKey));
        Guard.IsNotNullOrWhiteSpace(secretKey, nameof(secretKey));
        
        builder.Register(_ =>
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                return new AmazonSecurityTokenServiceClient(credentials);
            })
            .As<IAmazonSecurityTokenService>()
            .SingleInstance();


        return builder;

    }
    
    
    
    
    public static ContainerBuilder AddStsConfiguration(this ContainerBuilder builder, string roleArn, string policy, TimeSpan duration)
    {

        builder.Register( _ =>
            {
                var comp = new StsConfiguration
                {
                    RoleArn  = roleArn,
                    Policy   = policy,
                    Duration = duration
                };

                return comp;
            })
            .AsSelf()
            .SingleInstance();

        return builder;

    }


    public static ContainerBuilder AddDynamodbClient(this ContainerBuilder builder, string tablePrefix="" )
    {


        builder.Register(c =>
            {

                var credentials = c.ResolveOptional<AWSCredentials>();

                if( credentials is not null )
                    return new AmazonDynamoDBClient(credentials);


                return new AmazonDynamoDBClient();


            })
            .As<IAmazonDynamoDB>()
            .SingleInstance();


        builder.Register(c =>
            {

                var db = c.Resolve<IAmazonDynamoDB>();
                var dbc = new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => db )
                    .ConfigureContext(cfg =>
                    {
                        cfg.TableNamePrefix = tablePrefix;
                        cfg.IsEmptyStringValueEnabled = true;
                    })
                    .Build();

                return dbc;

            })
            .As<IDynamoDBContext>()
            .InstancePerLifetimeScope();


        return builder;


    }









}