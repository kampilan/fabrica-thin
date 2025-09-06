using Autofac;
using CommunityToolkit.Diagnostics;

namespace Fabrica.Utilities.Pipeline;

public static class AutofacExtensions
{

    public static ContainerBuilder AddPipelineBuilder<TBuilder,TContext>(this ContainerBuilder afb, Action<TBuilder> builder) where TBuilder : class, IPipelineBuilder<TContext>, new() where TContext : class, IPipelineContext
    {

        Guard.IsNotNull(afb, nameof(afb));
        Guard.IsNotNull(builder, nameof(builder));
        
        afb.Register(_ =>
        {

            var comp = new TBuilder();
            builder(comp);

            return comp;

        })
        .AsSelf()
        .As<IPipelineBuilder<TContext>>()
        .InstancePerDependency();

        return afb;
        
    }
    
    
    
}