
// ReSharper disable UnusedMember.Global

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fabrica.Identity.Gateway;


public class GatewayHeaderAuthenticationHandler : AuthenticationHandler<GatewayTokenAuthenticationSchemeOptions>
{


    public GatewayHeaderAuthenticationHandler( ICorrelation correlation,  IOptionsMonitor<GatewayTokenAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder )
    {
        Correlation = correlation;
    }

    private ICorrelation Correlation { get; }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        using var logger = Correlation.EnterMethod<GatewayTokenAuthenticationHandler>();

        
        // *****************************************************************
        logger.Debug("Attempting to dig out gateway header");
        var json = Context.Request.Headers[IdentityConstants.IdentityHeaderName].FirstOrDefault();
        if( string.IsNullOrWhiteSpace(json) )
        {
            logger.Debug("Header not present. Attempting to build skip result");
            var noresult = AuthenticateResult.NoResult();
            return Task.FromResult(noresult);
        }
        

        
        // *****************************************************************
        logger.Debug("Attempting to decode gateway header");
        var claims = JsonSerializer.Deserialize<ClaimSetModel>(json);
        if (claims is null)
        {
            logger.Debug( "JSON parse failed. Attempting to build skip result" );
            var noresult = AuthenticateResult.NoResult();
            return Task.FromResult(noresult);
        }            

        
        
        // *****************************************************************
        logger.Debug("Attempting to build ClaimsIdentity");
        var ci = new FabricaIdentity( claims );



        // *****************************************************************
        logger.Debug("Attempting to build ClaimsPrincipal");
        var cp = new ClaimsPrincipal(ci);



        // *****************************************************************
        logger.Debug("Attempting to build ticket and success result");
        var ticket = new AuthenticationTicket( cp, new AuthenticationProperties(), IdentityConstants.Scheme );
        var result = AuthenticateResult.Success(ticket);



        // *****************************************************************
        return Task.FromResult(result);


    }


}

