using System.Text.Json;
using Fabrica.Watch;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Identity.Token;


public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddGatewayAccessTokenSource(this IServiceCollection services, IClaimSet claims)
    {

        services.AddSingleton<IAccessTokenSource, GatewayAccessTokenSource>(sp =>
        {
            var encoder = sp.GetRequiredService<IGatewayTokenEncoder>();
            var comp = new GatewayAccessTokenSource(encoder, claims);

            return comp;

        });

        return services;

    }

}


public class GatewayAccessTokenSource(IGatewayTokenEncoder encoder, IClaimSet claims) : IAccessTokenSource
{

    private IGatewayTokenEncoder Encoder { get; } = encoder;
    private IClaimSet Claims { get; } = claims;

    public string Name { get; set; } = "";
    public bool HasExpired { get; set; }

    public Task<string> GetToken()
    {
        using var logger = this.EnterMethod();

        var json = JsonSerializer.Serialize(Claims);
        var cc = JsonSerializer.Deserialize<ClaimSetModel>(json)??new ClaimSetModel();

        var token = Encoder.Encode(cc);
        return Task.FromResult(token);

    }

    public Task CheckForRenewal(bool force = false)
    {
        return Task.CompletedTask;
    }
    
}