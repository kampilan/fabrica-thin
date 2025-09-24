using System.Collections.ObjectModel;
using System.Reflection;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Endpoints;


public static class EndpointExtensions
{

    public static IServiceCollection AddEndpointModules(this IServiceCollection services, params Assembly[] sources)
    {


        var assemblies = new ReadOnlyCollection<Assembly>(sources);

        var modules = assemblies.SelectMany(x => x.GetTypes().Where(t => !t.IsAbstract && typeof(IEndpointModule).IsAssignableFrom(t) && t != typeof(IEndpointModule) && (t.IsPublic || t.IsNested) ));

        foreach (var newModule in modules)
        {
            services.AddSingleton(typeof(IEndpointModule), newModule);
        }


        return services;

    }


    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder builder, string basePath="", Action<RouteGroupBuilder>? rootBuilder=null )
    {

        using var logger = WatchFactoryLocator.Factory.GetLogger(typeof(EndpointExtensions));


        var root = builder.MapGroup(basePath);
        if (rootBuilder is not null)
            rootBuilder(root);


        foreach (var module in builder.ServiceProvider.GetServices<IEndpointModule>())
        {

            try
            {

                logger.DebugFormat("Attempting to load Endpoint Module: {0}", module.GetType().GetConciseName());

                var group = root.MapGroup(module.BasePath);

                module.Configure(group);
                module.AddRoutes(group);

            }
            catch (Exception cause)
            {
                var ctx = new { Module = module.GetType().GetConciseName() };
                logger.ErrorWithContext(cause, ctx, "EndpointModule failed to load.");
            }

        }


        return builder;

    }


}

