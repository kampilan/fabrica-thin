using Amazon.DynamoDBv2.DataModel;
using Autofac;
using Fabrica.Aws.DynamoDb.Converters;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Identity.Client;

public static class AutofacExtensions
{

    public static ContainerBuilder AddDynCredentialGrantRepository( this ContainerBuilder builder )
    {

        builder.Register(c=>
        {

            var dbc = c.Resolve<IDynamoDBContext>();
            var comp = new DynCredentialGrantRepository(dbc);

            return comp;

        })
        .AsSelf()
        .As<ICredentialGrantRepository>()
        .SingleInstance();

        return builder;

    }
    
}


public class DynCredentialGrantRepository( IDynamoDBContext context ): ICredentialGrantRepository
{

    
    public Task<ICredentialGrant> CreateGrant( string name, string description, string metaEndpoint, CredentialGrantKind kind )
    {

        var grant = new DynCredentialGrant
        {

            Name = name,
            Description = description,
            MetaEndpoint = metaEndpoint,
            Kind = kind,
            AccessTokenUid = Ulid.NewUlid(DateTimeOffset.UnixEpoch),
            LastAccessUpdate = DateTime.UnixEpoch,
            LastRefreshUpdate = DateTime.UnixEpoch
           
        };
        
        return Task.FromResult<ICredentialGrant>(grant);
        
    }

    public async Task<ICredentialGrant> GetGrant(string name)
    {

        using var logger = this.EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to fetch Grant from DynamoDb");
        var grant = await context.LoadAsync<DynCredentialGrant>(name);
        
        
        // *************************************************        
        return grant;

    }

    public async Task UpdateGrant( ICredentialGrant grant )
    {
        
        using var logger = this.EnterMethod();

        
        // *************************************************        
        if( grant is DynCredentialGrant dg )
        {
            logger.Debug("Attempting to save Grant to DynamoDb");
            await context.SaveAsync(dg);
        }
        else
            throw new ArgumentException("Invalid grant type");
        
    }


    [DynamoDBTable( "credential-grants" )]
    private class DynCredentialGrant: ICredentialGrant
    {
    
        [DynamoDBHashKey]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    
        public string MetaEndpoint { get; set; } = string.Empty;

    
        [DynamoDBProperty("CredentialGrant",typeof(DynamoDbEnumConverter<CredentialGrantKind>))]
        public CredentialGrantKind Kind { get; set; }
    
        public string ClientId { get; set; } = string.Empty;
        [Sensitive]        
        public string ClientSecret { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        [Sensitive]
        public string Password { get; set; } = string.Empty;

        public string Scope { get; set; } = string.Empty;


        [DynamoDBProperty(nameof(Additional),typeof(DynamoDbDictionaryConverter))]
        public Dictionary<string, string> Additional { get; set; } = [];
        
        public string AccessTokenUid { get; set; } = string.Empty;
        [Sensitive]
        public string AccessToken { get; set; } = string.Empty;
        [DynamoDBProperty(typeof(DynamoDbTimeSpanConverter))]    
        public TimeSpan AccessTimeToLive { get; set; }
        [DynamoDBProperty(nameof(LastAccessUpdate), typeof(DynamoDbDataTimeConverter))]
        public DateTime LastAccessUpdate { get; set; }

        public string RefreshExpirationExtension { get; set; } = string.Empty;
        [Sensitive]
        public string RefreshToken { get; set; } = string.Empty;
        [DynamoDBProperty(typeof(DynamoDbTimeSpanConverter))]
        public TimeSpan RefreshTimeToLive { get; set; }
        [DynamoDBProperty(nameof(LastRefreshUpdate), typeof(DynamoDbDataTimeConverter))]
        public DateTime LastRefreshUpdate { get; set; }

        
    }    
    
    
}





