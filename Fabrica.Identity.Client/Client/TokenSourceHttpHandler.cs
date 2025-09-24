using System.Net.Http.Headers;
using CommunityToolkit.Diagnostics;
using Fabrica.Watch;

namespace Fabrica.Identity.Client;

/// <summary>
/// A delegating handler that automatically attaches a Bearer token to outgoing HTTP requests.
/// </summary>
/// <remarks>
/// This handler retrieves the Bearer token from an implementation of <see cref="ITokenSource"/>
/// and appends it to the Authorization header of the HTTP request.
/// </remarks>
/// <example>
/// Useful in scenarios where authorization is required for making API calls and the token lifecycle
/// is managed separately through the <see cref="ITokenSource"/>.
/// </example>
/// <seealso cref="ITokenSource"/>
public class TokenSourceHttpHandler(ITokenSource source) : DelegatingHandler
{

    /// <summary>
    /// Sends an HTTP request with an attached Bearer authorization token if it is available,
    /// and returns the HTTP response.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        Guard.IsNotNull(source, nameof(source));

        using var logger = this.EnterMethod();
        
        var token = await source.GetToken().ConfigureAwait(false);
        
        logger.Inspect(nameof(token), token);

        if( !string.IsNullOrWhiteSpace(token) )
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        
    }
    
}
