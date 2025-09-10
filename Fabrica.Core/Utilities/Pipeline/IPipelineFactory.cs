﻿namespace Fabrica.Utilities.Pipeline;

public interface IPipelineFactory
{
    Pipeline<TContext> Create<TContext>() where TContext : class, IPipelineContext;
}