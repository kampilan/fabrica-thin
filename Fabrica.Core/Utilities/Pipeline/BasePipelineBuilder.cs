using CommunityToolkit.Diagnostics;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public abstract class BasePipelineBuilder<TContext>: IPipelineBuilder<TContext> where TContext : class, IPipelineContext
{

    private readonly List<IPipelineStep<TContext>> _steps = new();

    public IPipelineBuilder<TContext> AddStep(IPipelineStep<TContext> step)
    {

        Guard.IsNotNull(step, nameof(step));
        
        _steps.Add(step);
        return this;
        
    }

    public virtual Pipeline<TContext> Build()
    {

        Func<TContext, Task> next = (ctx) => Task.CompletedTask;

        foreach (var step in _steps.AsEnumerable().Reverse())
        {
            var currentStep = step;
            var capturedNext = next;
            next = async (ctx) => await InvokeWapper(currentStep, ctx, capturedNext);
        }

        return new Pipeline<TContext>(next);
        
        async Task InvokeWapper( IPipelineStep<TContext> step, TContext context, Func<TContext,Task> next )
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