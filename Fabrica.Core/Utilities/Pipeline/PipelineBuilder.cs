using CommunityToolkit.Diagnostics;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public sealed class PipelineBuilder<TContext>: IPipelineBuilder<TContext> where TContext : class, IPipelineContext
{

    private readonly List<IPipelineStep<TContext>> _steps = new();
    private bool _closed;

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

        if(!_closed)
            _steps.Add(step);
        else
            throw new InvalidOperationException("Cannot add steps to a closed pipeline builder");
        
        return this;
        
    }

    /// <summary>
    /// Sets the primary action to be executed within the pipeline.
    /// This action must be provided as a function that takes a pipeline context
    /// and returns a <see cref="Task"/> for asynchronous execution.
    /// </summary>
    /// <param name="action">
    /// A function that specifies the main operation of the pipeline.
    /// This function accepts an instance of <typeparamref name="TContext"/>
    /// as its parameter and returns a <see cref="Task"/> representing the asynchronous operation.
    /// </param>
    public void SetAction( Func<TContext,Task> action )
    {
        var step = new ActionPipelineStep<TContext>(action);
        AddStep(step);
        _closed = true;
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

        if( !_closed )
            throw new InvalidOperationException("Cannot build an unclosed pipeline builder. This indicates that no action was set.");

        
        Func<TContext, Task> nextAction = (_) => Task.CompletedTask;

        foreach (var step in _steps.AsEnumerable().Reverse())
        {
            var currentStep = step;
            var capturedNext = nextAction;
            nextAction = async (ctx) => await InvokeWrapper(currentStep, ctx, capturedNext);
        }

        return new Pipeline<TContext>(nextAction);
        
        async Task InvokeWrapper( IPipelineStep<TContext> step, TContext context, Func<TContext,Task> next )
        {

            try
            {
                await step.InvokeAsync(context, next);
            }
            catch( Exception cause )
            {

                if( context.Success )
                {
                    context.Success = false;
                    context.FailedStep = step.GetType().GetConciseFullName();
                    context.Cause = cause;
                }

                using var logger = this.GetLogger();
                logger.Error(cause, $"Exception encountered: Failed to execute pipeline in Step: ({context.FailedStep}) in Phase: ({context.Phase})");                
                
            }
            
        }
        
        
    }
    
}