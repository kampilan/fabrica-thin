using System.Collections.ObjectModel;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Identity.Client;

public enum CredentialGrantKind { ClientCredentials, ResourceOwner, RefreshToken }

public interface ICredentialGrant
{

    CredentialGrantKind Kind { get; }
    
    string Name { get; }
    string Description { get; }

    string MetaEndpoint { get; }

    string ClientId { get; }
    string ClientSecret { get; }
    
    string UserName { get; }
    string Password { get; }

    string Scope { get; }
    
    Dictionary<string, string> Additional { get; }
    
    string RefreshExpirationExtension { get; set; }    
    string RefreshToken { get; set; }
    TimeSpan RefreshTimeToLive { get; set; }
    DateTime LastRefreshUpdate { get; set; }    
    
    string AccessTokenUid { get; set; }   
    string AccessToken { get; set; }
    TimeSpan AccessTimeToLive { get; set; }
    DateTime LastAccessUpdate { get; set; }

    bool HasAccessTokenExpired => LastAccessUpdate + AccessTimeToLive < DateTime.Now;
    bool HasRefreshTokenExpired => LastRefreshUpdate + RefreshTimeToLive < DateTime.Now;
    
    void UpdateAccessToken( string accessToken, TimeSpan accessTtl, string refreshToken, TimeSpan refreshTtl )
    {

        AccessTokenUid   = Ulid.NewUlid();       
        AccessToken      = accessToken;
        AccessTimeToLive = accessTtl;
        LastAccessUpdate = DateTime.Now;

        if( refreshToken == RefreshToken ) 
            return;

        RefreshToken      = refreshToken;
        RefreshTimeToLive = refreshTtl;
        LastRefreshUpdate = DateTime.Now;
        
    }

    IReadOnlyDictionary<string, string> ToBody()
    {

        var body = new Dictionary<string, string>();
        switch( Kind )
        {
            case CredentialGrantKind.ClientCredentials:

                body["grant_type"] = "client_credentials";
                break;
            
            case CredentialGrantKind.ResourceOwner when HasRefreshTokenExpired:

                body["grant_type"] = "password";
                body["username"] = UserName;
                body["password"] = Password;
                break;
            
            case CredentialGrantKind.ResourceOwner when !HasRefreshTokenExpired:
            case CredentialGrantKind.RefreshToken:

                body["grant_type"] = "refresh_token";
                body["refresh_token"] = RefreshToken;
                break;
            
        }

        body["client_id"] = ClientId;
        body["client_secret"] = ClientSecret;

        
        if( !string.IsNullOrWhiteSpace( Scope ) )
            body["scope"] = Scope;

        
        foreach( var p in Additional)
            body[p.Key] = p.Value;
        
        return new ReadOnlyDictionary<string, string>(body);        
        
        
    }
    
    
    
}


public class CredentialGrant : ICredentialGrant
{
    
    public CredentialGrantKind Kind { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string MetaEndpoint { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;
    [Sensitive]
    public string ClientSecret { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    [Sensitive]
    public string Password { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public Dictionary<string, string> Additional { get; } = new();

    public string RefreshExpirationExtension { get; set; } = string.Empty;
    [Sensitive]
    public string RefreshToken { get; set; } = string.Empty;
    public TimeSpan RefreshTimeToLive { get; set; }
    public DateTime LastRefreshUpdate { get; set; } = DateTime.MinValue;    

    public string AccessTokenUid { get; set; } = Ulid.NewUlid(DateTimeOffset.UnixEpoch);
    [Sensitive]
    public string AccessToken { get; set; } = string.Empty;
    public TimeSpan AccessTimeToLive { get; set; }
    public DateTime LastAccessUpdate { get; set; } = DateTime.MinValue;    
    
    
}