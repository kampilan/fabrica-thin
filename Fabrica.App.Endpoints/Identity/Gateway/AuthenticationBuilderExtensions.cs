using Microsoft.AspNetCore.Authentication;

namespace Fabrica.App.Identity.Gateway;

public static class AuthenticationBuilderExtensions
{

    public static AuthenticationBuilder AddGatewayToken( this AuthenticationBuilder builder )
    {

        builder.AddScheme<GatewayTokenAuthenticationSchemeOptions, GatewayTokenAuthenticationHandler>( IdentityConstants.Scheme, _ => { } );

        return builder;

    }

    public static AuthenticationBuilder AddGatewayHeader( this AuthenticationBuilder builder )
    {

        builder.AddScheme<GatewayTokenAuthenticationSchemeOptions, GatewayHeaderAuthenticationHandler>( IdentityConstants.Scheme, _ => { } );

        return builder;

    }
    
    
}