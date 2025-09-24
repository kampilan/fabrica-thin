using System.Net.Http.Headers;
using CommunityToolkit.Diagnostics;
using Fabrica.Identity;
using JetBrains.Annotations;

namespace Fabrica.App.Identity.Token;

[UsedImplicitly]
public class GatewayTokenHttpHandler( IAccessTokenSource source ): DelegatingHandler
{
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        Guard.IsNotNull(source, nameof(source));
        
        var token = await source.GetToken().ConfigureAwait(false);

        if( !string.IsNullOrWhiteSpace(token) )
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        
    }    
    
    
}