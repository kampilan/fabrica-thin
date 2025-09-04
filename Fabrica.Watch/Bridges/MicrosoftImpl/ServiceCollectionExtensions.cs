using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddWatchMsLoggerFactory(this IServiceCollection services)
    {

        services.AddScoped<ILoggerFactory, LoggerFactory>(p =>
        {

            var correlation = p.GetService<ILoggingCorrelation>();
            var comp = new LoggerFactory(correlation);

            return comp;
            
        });
        
        return services;
        
    }
    
}