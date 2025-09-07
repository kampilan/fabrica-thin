namespace Fabrica.Utilities.Pipeline;

public interface IPipelineBuilder<TContext>  where TContext : class, IPipelineContext
{
    /// <summary>
    /// Sets the action to be executed as part of the pipeline process.
    /// This action represents a pipeline step that will be added to the pipeline.
    /// </summary>
    /// <param name="action">A function representing the action to be executed, with the pipeline context as the input parameter.</param>
    void SetAction(Func<TContext, Task> action);

    /// <summary>
    /// Constructs and finalizes the pipeline composed of user-provided steps and actions.
    /// The pipeline is built in the order in which steps are added, following a reverse execution flow.
    /// </summary>
    /// <returns>A fully constructed <see cref="Pipeline{TContext}"/> instance, ready to execute the configured steps and actions.</returns>
    Pipeline<TContext> Build();
    

}