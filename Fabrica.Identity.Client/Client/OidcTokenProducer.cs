using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;
using Fabrica.Watch;

namespace Fabrica.Identity.Client;

public class OidcTokenProducer(IHttpClientFactory factory) : ITokenProducer
{
 
    public async Task<AccessToken> FetchNew( ICredentialGrant grant )
    {
        
        Guard.IsNotNull(grant, nameof(grant));
        
        using var logger = this.EnterMethod();

        logger.LogObject(nameof(grant), grant);

        
        // *************************************************
        logger.Debug("Attempting to fetch ProviderMeta");
        var providerMeta = await _fetchProvider(grant);


        
        // *************************************************
        logger.DebugFormat("Attempting to fetch new AccessToken for Grant '{0}'", grant.Name );        
        var token = await _fetchToken( grant, providerMeta );

        
        
        // *************************************************        
        return token;

    }

    public AccessToken FromGrant( ICredentialGrant grant )
    {

        Guard.IsNotNull(grant, nameof(grant));        

        using var logger = this.EnterMethod();

        logger.LogObject(nameof(grant), grant);
        
        var expiration = grant.LastAccessUpdate + grant.AccessTimeToLive;
        return new AccessToken(grant.AccessToken, expiration);
        
    }

    private async Task<ProviderMeta> _fetchProvider(ICredentialGrant grant)
    {

        Guard.IsNotNull(grant, nameof(grant));        
        
        using var logger = this.EnterMethod();

        using var client = factory.CreateClient();

        
        // *****************************************************************
        logger.Debug("Attempting to Fetch provider meta");
        var phase = "HTTP Call";
        try
        {

            
            // *************************************************
            logger.Debug("Attempting to get meta via HTTP Get");
            var res = await client.GetAsync( grant.MetaEndpoint );

            logger.Inspect(nameof(res.StatusCode), res.StatusCode);

            res.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to build read JSON");
            phase = "Read JSON";           
            var json = await res.Content.ReadAsStringAsync();

            logger.LogJson("ProviderMeta JSON", json);



            // *****************************************************************
            logger.Debug("Attempting to build ProviderMeta from JSON");
            phase = "Build ProviderMeta";           
            var meta = JsonSerializer.Deserialize<ProviderMeta>(json);

            if( meta is null )
                throw new Exception( "Null ProviderMeta after JSON parse." );

            logger.LogObject(nameof(ProviderMeta), meta);

            return meta;


        }
        catch (Exception cause)
        {
            logger.Error(cause, $"Get Provider Meta failed for Grant '{grant.Name}' during '{phase}'");
            throw;
        }        
        
        
    }
    
    private async Task<AccessToken> _fetchToken( ICredentialGrant grant, ProviderMeta providerMeta )
    {

        Guard.IsNotNull(grant, nameof(grant));
        Guard.IsNotNull(providerMeta, nameof(providerMeta));
        
        using var logger = this.EnterMethod();


        using var client = factory.CreateClient();

        
        // *************************************************
        logger.Debug("Attempting to Fetch new access token");
        var phase = "Get body from Grant";       
        try
        {

            
            // *************************************************
            logger.Debug("Attempting to build request body for grant");
            var body = grant.ToBody();

            logger.LogObject(nameof(body), body);
            
            
            
            // *****************************************************************
            logger.Debug("Attempting to build HTTP Content for grant");
            phase = "Build HTTP Content";           
            var content = new FormUrlEncodedContent(body);
            

            
            // *****************************************************************
            logger.Debug("Attempting to fetch tokens from identity provider");
            phase = "HTTP Call";           
            var res = await client.PostAsync( providerMeta.TokenEndpoint, content );

            logger.Inspect(nameof(res.StatusCode), res.StatusCode);

            res.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to get token JSON from response");
            phase = "Read JSON";           
            var json = await res.Content.ReadAsStringAsync();



            // *****************************************************************
            logger.Debug("Attempting to build TokenResponse from JSON");
            phase = "Build TokenResponse";           
            var response = JsonSerializer.Deserialize<TokenResponse>(json);

            if( response is null )
                throw new Exception("Null TokenResponse encountered after JSON parse");

            logger.LogObject( nameof(response), response );


            
            // *************************************************
            logger.Debug("Attempting to try to dig out refresh token TTL");
            phase = "Calculate Refresh TTL";           
            var refreshTtl = TimeSpan.FromHours(24);
            if( response.Extensions.TryGetValue(grant.RefreshExpirationExtension, out var obj)  )
            {
                var ttl = obj.GetInt32();
                refreshTtl = TimeSpan.FromSeconds(ttl);                
            }
            

            
            // *************************************************
            logger.Debug("Attempting to update the grant with the new token");
            phase = "Update Grant";           
            grant.UpdateAccessToken( response.AccessToken, response.GetAccessTokenTtl(), response.RefreshToken, refreshTtl );
            

            
            // *****************************************************************            
            var token = new AccessToken( response.AccessToken, grant.LastAccessUpdate + grant.AccessTimeToLive );
            return token;            
            
            
        }
        catch (Exception cause)
        {
            logger.Error(cause, $"Fetch new access token failed for Grant '{grant.Name}' during '{phase}'");
            throw;
        }


    }    

    
    private class ProviderMeta
    {
    
        [JsonPropertyName("issuer")]
        public string Issuer { get; init; } = string.Empty;

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; init; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; init; } = string.Empty;

        [JsonPropertyName("introspection_endpoint")]
        public string IntrospectionEndpoint { get; init; } = string.Empty;

        [JsonPropertyName("userinfo_endpoint")]
        public string UserInfoEndpoint { get; init; } = string.Empty;

    }    
    
    private class TokenResponse
    {

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = "";

        [Sensitive]
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [Sensitive]
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; init; } = "";

        [JsonPropertyName("scope")]
        public string Scope { get; init; } = "";
        
        [JsonExtensionData]
        public Dictionary<string,JsonElement> Extensions { get; init; } = [];
        
        
        private readonly DateTime _created = DateTime.Now;
        public TimeSpan GetAccessTokenTtl()
        {

            var millis = TimeSpan.FromSeconds(ExpiresIn).TotalMilliseconds * .95;
            var ttl = TimeSpan.FromMilliseconds(millis);            
            
            return ttl;
            
        }


    }
    
}



