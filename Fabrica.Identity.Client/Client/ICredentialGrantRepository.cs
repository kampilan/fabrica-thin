namespace Fabrica.Identity.Client;

public interface ICredentialGrantRepository
{
    Task<ICredentialGrant> CreateGrant( string name, string description, string metaEndpoint, CredentialGrantKind kind  );
    Task<ICredentialGrant> GetGrant(string name);
    Task UpdateGrant(ICredentialGrant grant);

}