using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.App.Identity.Token;
using Fabrica.Utilities.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.App.One;

public static class AutofacExtensions
{

    public static ContainerBuilder AddMissionServiceEndpoints(this ContainerBuilder builder, IMissionContext context)
    {

        var services = new ServiceCollection();
        foreach( var pair in context.ServiceEndpoints )
        {

            services.AddHttpClient(pair.Key, c =>
                {
                    c.BaseAddress = new Uri(pair.Value);
                })
                .AddHttpMessageHandler<GatewayTokenHttpHandler>();

        }

        builder.Populate(services);
        
        return builder;

    }    
    
}