namespace Fabrica.Utilities.Pipeline;

public interface IPipelineStep<TContext> where TContext : class
{
    Task InvokeAsync(TContext context, Func<TContext, Task> next);
    
}