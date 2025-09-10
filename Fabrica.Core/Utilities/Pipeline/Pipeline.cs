using CommunityToolkit.Diagnostics;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public class Pipeline<TContext> where TContext : class, IPipelineContext
{

    private readonly ICollection<IPipelineStep<TContext>> _steps;

    internal Pipeline(ICollection<IPipelineStep<TContext>> steps )
    {
        _steps = steps;
    }

    public async Task ExecuteAsync( TContext context, Func<TContext, Task> action )
    {
        
        Guard.IsNotNull(context, nameof(context));        
        
        using var logger = this.EnterMethod();

        try
        {

            var innerSteps = _steps.ToList();
            innerSteps.Add(new ActionPipelineStep<TContext>(action));
            
            Func<TContext, Task> nextAction = (_) => Task.CompletedTask;
            
            foreach( var step in innerSteps.AsEnumerable().Reverse() )
            {
                var currentStep = step;
                var capturedNext = nextAction;
                nextAction = async (ctx) => await InvokeWrapper(currentStep, ctx, capturedNext);
            }
            
            
            // *************************************************
            logger.Debug("Attempting to execute pipeline");
            await nextAction(context);

            
        }
        catch (Exception cause)
        {
            logger.Error(cause, "Unhandled Exception encountered: Failed to execute pipeline");
            throw;
        }        

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