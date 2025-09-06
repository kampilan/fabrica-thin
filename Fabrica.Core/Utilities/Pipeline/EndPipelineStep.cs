using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public class EndPipelineStep<TContext>( Func<TContext,Task> action ): IPipelineStep<TContext> where TContext : class, IPipelineContext
{

    public async Task InvokeAsync(TContext context, Func<TContext, Task> next)
    {

        using var logger = this.EnterMethod();

        await action(context);
        
    }

    
}