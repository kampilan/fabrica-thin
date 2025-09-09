namespace Fabrica.Identity.Client;

public interface ITokenProducer
{

    Task<AccessToken> FetchNew(ICredentialGrant grant);
    AccessToken FromGrant(ICredentialGrant grant);
    
}

public sealed record AccessToken(string Token, DateTime Expires);