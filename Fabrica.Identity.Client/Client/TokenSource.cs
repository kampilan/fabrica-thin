using Fabrica.Utilities.Cache;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Identity.Client;

public class TokenSource : CorrelatedObject, ITokenSource, IRequiresStart
{

    
    public TokenSource( string grantName, ICredentialGrantRepository repository, ITokenProducer tokenProducer ) : base( new Correlation() )
    {

        GrantName    = grantName;      
        TokenProducer = tokenProducer;      
        Repository   = repository;

        _cache = new ConcurrentResource<AccessToken>( _renewToken );

    }
    
    public TokenSource( ICorrelation correlation, string grantName, ICredentialGrantRepository repository, ITokenProducer tokenProducer ) : base(correlation)
    {

        GrantName    = grantName;      
        TokenProducer = tokenProducer;      
        Repository   = repository;

        _cache = new ConcurrentResource<AccessToken>( _renewToken );

    }
    
    
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public TimeSpan RenewalWindow { get; set; } = TimeSpan.FromSeconds(60);
    public string Name => Grant.Name;

    private ICredentialGrantRepository Repository { get; }
    private ITokenProducer TokenProducer { get; }

    private string GrantName { get; }    
    private ICredentialGrant Grant { get; set; } = new CredentialGrant();
    

    public async Task Start()
    {

        using var logger = EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to initialize cache");
        await _cache.Initialize();

        
    }

    private readonly ConcurrentResource<AccessToken> _cache;
    public bool HasExpired => _cache.HasExpired;
    public int RenewCount => _cache.RenewCount;

    public async Task<string> GetToken()
    {

        using var logger = EnterMethod();

        
        // *************************************************
        logger.Debug("Attempting to get AccessTokenModel from cache");
        var accessTokenModel = await _cache.GetResource();

        
        // *************************************************
        return accessTokenModel.Token;

    }

    public async Task CheckForRenewal( bool force = false )
    {

        using var logger = EnterMethod();
        
        await _cache.CheckForRenewal(force);

    }


    private async Task<IRenewedResource<AccessToken>> _renewToken()
    {

        using var logger = EnterMethod();

        
        
        // *************************************************
        logger.Debug("Attempting to get Grant from repository");
        try
        {
            Grant = await Repository.GetGrant( GrantName );
        }
        catch (Exception cause)
        {
            logger.Error(cause, $"Get Grant failed for GrantName '{GrantName}'");
            throw;
        }        

        
        
        // *************************************************
        logger.Debug("Attempting to check if access token has already been updated");
        if( !Grant.HasAccessTokenExpired )
        {

            var token = TokenProducer.FromGrant(Grant);
                
            var resource = new RenewedResource<AccessToken>
            {
                Value       = token, 
                TimeToLive  = Grant.AccessTimeToLive,
                TimeToRenew = Grant.AccessTimeToLive - RenewalWindow
            };
                
            return resource;
                
        }            
        
        
        try
        {
            
            // *************************************************
            logger.Debug("Attempting to get new AccessToken from Factory");
            var fetched = await TokenProducer.FetchNew(Grant);


            // *************************************************
            logger.Debug("Attempting to Update the grant with the Repository");
            await Repository.UpdateGrant(Grant);

        
            // *****************************************************************
            return new RenewedResource<AccessToken>{ Value = fetched, TimeToLive = Grant.AccessTimeToLive, TimeToRenew = Grant.AccessTimeToLive - RenewalWindow };
            
        }
        catch (Exception cause)
        {
            logger.Error(cause, $"Fetch token failed for GrantName '{Grant.Name}'");
            throw;
        }


    }


}

