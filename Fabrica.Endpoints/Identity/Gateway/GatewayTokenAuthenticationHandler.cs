﻿
// ReSharper disable UnusedMember.Global

using System.Security.Claims;
using System.Text.Encodings.Web;
using Fabrica.Identity.Token;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fabrica.Identity.Gateway;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddGatewayTokenAuthentication(this IServiceCollection services, string tokenSigningKey )
    {

        services.AddSingleton<IGatewayTokenEncoder>(_ =>
        {

            byte[] key = null!;
            if (!string.IsNullOrWhiteSpace(tokenSigningKey))
                key = Convert.FromBase64String(tokenSigningKey);

            var comp = new GatewayTokenJwtEncoder
            {
                GatewayTokenSigningKey = key
            };

            return comp;

        });

        services.AddAuthentication(op =>
            {
                op.DefaultScheme = IdentityConstants.Scheme;
            })
            .AddGatewayToken();

        return services;

    }

}

public static class AuthenticationBuilderExtensions
{

    public static AuthenticationBuilder AddGatewayToken( this AuthenticationBuilder builder )
    {

        builder.AddScheme<GatewayTokenAuthenticationSchemeOptions, GatewayTokenAuthenticationHandler>( IdentityConstants.Scheme, _ => { } );

        return builder;

    }

}


public class GatewayTokenAuthenticationHandler : AuthenticationHandler<GatewayTokenAuthenticationSchemeOptions>
{


    public GatewayTokenAuthenticationHandler( ICorrelation correlation, IGatewayTokenEncoder jwtEncoder, IOptionsMonitor<GatewayTokenAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder )
    {

        Correlation = correlation;
        JwtEncoder  = jwtEncoder;

    }

    private ICorrelation Correlation { get; }
    private IGatewayTokenEncoder JwtEncoder { get; }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {

        using var logger = Correlation.EnterMethod<GatewayTokenAuthenticationHandler>();


        var token = Context.Request.Headers[IdentityConstants.TokenHeaderName].FirstOrDefault();
        if( string.IsNullOrWhiteSpace(token) )
        {
            logger.Debug("Header not present. Attempting to build skip result");
            var noresult = AuthenticateResult.NoResult();
            return Task.FromResult(noresult);
        }



        IClaimSet claims;
        // *****************************************************************
        logger.Debug("Attempting to decode gateway token");
        try
        {

            claims = JwtEncoder.Decode( IdentityConstants.Scheme, token );

            logger.LogObject(nameof(claims), claims);

        }
        catch (Exception cause)
        {
            logger.Debug( cause, "Decode failed. Attempting to build skip result" );
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
        logger.Debug("Attempting to set Caller on Correlation");
        if ( Correlation is Correlation impl )
        {
            impl.CallerGatewayToken = token;
            impl.Caller = result.Principal!;
        }


        // *****************************************************************
        return Task.FromResult(result);


    }


}