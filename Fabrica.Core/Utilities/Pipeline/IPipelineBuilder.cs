﻿namespace Fabrica.Utilities.Pipeline;

internal interface IPipelineBuilder<TContext>  where TContext : class, IPipelineContext
{

    /// <summary>
    /// Constructs and finalizes the pipeline composed of user-provided steps and actions.
    /// The pipeline is built in the order in which steps are added, following a reverse execution flow.
    /// </summary>
    /// <returns>A fully constructed <see cref="Pipeline{TContext}"/> instance, ready to execute the configured steps and actions.</returns>
    Pipeline<TContext> Build();
    

}