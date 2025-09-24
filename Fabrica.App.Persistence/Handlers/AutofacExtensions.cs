

// ReSharper disable UnusedMethodReturnValue.Global

using Autofac;
using JetBrains.Annotations;

namespace Fabrica.App.Handlers;

[UsedImplicitly]
public static class AutofacExtensions
{
    
    public static ContainerBuilder RegisterHandlerServices(this ContainerBuilder builder)
    {
        
        
        builder.RegisterType<QueryService>()
            .As<IQueryService>()
            .InstancePerDependency();


        builder.RegisterType<CommandService>()
            .As<ICommandService>()
            .InstancePerDependency();

        return builder;

    }


}