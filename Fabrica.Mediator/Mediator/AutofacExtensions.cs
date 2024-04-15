using Autofac;
using Fabrica.Rules;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Mediator;

public static class AutofacExtensions
{

    public static ContainerBuilder RegisterRequestMediator(this ContainerBuilder builder)
    {

        builder.Register(c =>
            {

                var scope = c.Resolve<ILifetimeScope>();
                var rules = c.Resolve<IRuleSet>();

                var comp = new RequestMediator(scope, rules);

                return comp;

            })
            .As<IRequestMediator>()
            .InstancePerLifetimeScope();


        return builder;

    }




}