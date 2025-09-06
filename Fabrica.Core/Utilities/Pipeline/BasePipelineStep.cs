using CommunityToolkit.Diagnostics;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public abstract class BasePipelineStep<TContext> where TContext : class, IPipelineContext
{

    protected bool ContinueOnFailure { get; set; }
    
    public async Task InvokeAsync(TContext context, Func<TContext, Task> next)
    {

        Guard.IsNotNull(context, nameof(context));
        Guard.IsNotNull(next, nameof(next));
        
        if (!ContinueOnFailure && !context.Success)
            return;

        
        await Before(context);
        
        await next(context);
        
        if (!ContinueOnFailure && !context.Success )
            return;
        
        await After(context);        

    }

    protected virtual Task Before(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));
        
        return Task.CompletedTask;        
        
    }    
    
    protected virtual Task After(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));
        
        return Task.CompletedTask;
        
    }    
    
    
}