﻿using Autofac;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public class PipelineFactory(ILifetimeScope scope): IPipelineFactory
{

    
    public Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext
    {

        using var logger = this.EnterMethod();

        var builder = scope.Resolve<IPipelineBuilder<TContext>>();
        var pipeline = builder.Build();
        
        return pipeline;

    }
    
}