using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

/// Represents a pipeline step that performs an asynchronous action on the given context.
/// Implements the IPipelineStep interface and is used to execute a provided action
/// within a pipeline.
/// This class utilizes logging capabilities to trace method execution and modifies the context
/// to indicate the phase of pipeline execution.
/// Type Parameters:
/// TContext:
/// The type of the pipeline context. Must implement the IPipelineContext interface.
/// Constructors:
/// ActionPipelineStep(Func<TContext, Task> action):
/// Creates an instance of ActionPipelineStep with the specified action to execute.
/// Methods:
/// Task InvokeAsync(TContext context, Func<TContext, Task> next):
/// Invokes the provided action on the given context and then updates the context phase to "After".
/// Does not call the next pipeline step.
internal class ActionPipelineStep<TContext>( Func<TContext,Task> action ): IPipelineStep<TContext> where TContext : class, IPipelineContext
{
    
    public bool ContinueAfterFailure { get; set; }

    public async Task InvokeAsync( TContext context, Func<TContext,Task> next )
    {

        using var logger = this.EnterMethod();

        await action(context);
        context.Phase = PipelinePhase.After;

    }

    
}