using System.Net.Http.Headers;
using Autofac.Features.AttributeFilters;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace Fabrica.Identity.Token;

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