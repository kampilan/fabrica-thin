﻿using CommunityToolkit.Diagnostics;

namespace Fabrica.Utilities.Pipeline;

internal sealed class PipelineBuilder<TContext>: IPipelineBuilder<TContext> where TContext : class, IPipelineContext
{

    private readonly List<IPipelineStep<TContext>> _steps = new();

    /// <summary>
    /// Adds a processing step to the pipeline. The provided step will be executed
    /// as part of the pipeline in the order it is added.
    /// </summary>
    /// <param name="step">
    /// The pipeline step to be added. This step must implement <see cref="IPipelineStep{TContext}"/>
    /// and define its behavior for processing the context.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="IPipelineBuilder{TContext}"/> instance to allow for method chaining.
    /// </returns>
    internal IPipelineBuilder<TContext> AddStep(IPipelineStep<TContext> step)
    {

        Guard.IsNotNull(step, nameof(step));

        _steps.Add(step);
        
        return this;
        
    }

    
    /// <summary>
    /// Builds and returns a fully constructed pipeline instance. The pipeline consists of
    /// the configured steps, executed in the order they were added, and provides a mechanism
    /// to process a context through the pipeline.
    /// </summary>
    /// <returns>
    /// Returns an instance of <see cref="Pipeline{TContext}"/> that represents the configured
    /// pipeline ready for execution.
    /// </returns>
    public Pipeline<TContext> Build()
    {

        return new Pipeline<TContext>(_steps);
        
    }
    
}