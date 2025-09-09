using System.Collections.Concurrent;
using Fabrica.Utilities.Types;

namespace Fabrica.Identity.Client;

public class MemoryCredentialGrantRepository : ICredentialGrantRepository
{
    
    private readonly ConcurrentDictionary<string, ICredentialGrant> _grants = new ();
    
    public void AddGrant(ICredentialGrant grant)
    {
        _grants.AddOrUpdate( grant.Name, grant, (_, _) => grant );
    }

    public Task<ICredentialGrant> CreateGrant( string name, string description, string metaEndpoint, CredentialGrantKind kind )
    {

        var grant = new CredentialGrant
        {

            Name = name,
            Description = description,
            MetaEndpoint = metaEndpoint,
            Kind = kind,

            AccessTokenUid = Ulid.NewUlid(DateTimeOffset.UnixEpoch),
            LastAccessUpdate = DateTime.UnixEpoch,
            LastRefreshUpdate = DateTime.UnixEpoch,
            
        };
        
        return Task.FromResult<ICredentialGrant>(grant);
        
    }

    public Task<ICredentialGrant> GetGrant(string name)
    {

        if( _grants.TryGetValue(name, out var grant) )
            return Task.FromResult(grant);

        throw new Exception($"Could not find grant with name '{name}'    ");
        
    }

    public Task UpdateGrant( ICredentialGrant grant )
    {
        return Task.CompletedTask;
    }

    
}