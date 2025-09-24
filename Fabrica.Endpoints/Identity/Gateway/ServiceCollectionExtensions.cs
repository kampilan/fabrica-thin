using Fabrica.Identity.Token;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;

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


        services.AddKeyedScoped<IAccessTokenSource>("Fabrica.Gateway", (c,_) =>
        {
            var corr = c.GetRequiredService<ICorrelation>();
            var comp = new GatewayAmbientTokenSource(corr);
            return comp;

        });

        services.AddTransient<GatewayTokenHttpHandler>((s) =>
        {

            var source = s.GetRequiredKeyedService<IAccessTokenSource>("Fabrica.Gateway");
            var handler = new GatewayTokenHttpHandler(source);

            return handler;

        });
        
        
        return services;

    }

    public static IServiceCollection AddGatewayHeaderAuthentication(this IServiceCollection services )
    {

        services.AddAuthentication(op =>
            {
                op.DefaultScheme = IdentityConstants.Scheme;
            })
            .AddGatewayHeader();

        return services;

    }    
    
    
    
    
    
}