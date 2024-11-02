using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Mediator;

public static class AutofacExtensions
{

    public static ContainerBuilder RegisterRequestMediator(this ContainerBuilder builder, params Assembly[] sources )
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

                var comp = new RequestMediator(scope, corr, rules);

                return comp;

            })
            .As<IRequestMediator>()
            .InstancePerLifetimeScope();


        return builder;

    }




}

