using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Mediator;

public static class AutofacExtensions
{

    public static ContainerBuilder RegisterRequestMediator(this ContainerBuilder builder, params Assembly[] sources)
    {
        return RegisterRequestMediator<RequestMediator>(builder, sources);
    }

    public static ContainerBuilder RegisterRequestMediator<T>(this ContainerBuilder builder, params Assembly[] sources ) where T : AbstractRequestMediator, new()
    {

        var services = new ServiceCollection();
        services.AddMediatR(c =>
        {
            c.RegisterServicesFromAssemblies(sources);
        });

        builder.Populate(services);


        builder.Register(c =>
            {

                var scope = c.Resolve<ILifetimeScope>();
                var corr  = c.Resolve<ICorrelation>();
                var rules = c.Resolve<IRuleSet>();

                var comp = new T();
                comp.Configure(scope, corr, rules);

                return comp;

            })
            .As<IRequestMediator>()
            .InstancePerLifetimeScope();


        return builder;

    }




}

