using CommunityToolkit.Diagnostics;
using Fabrica.Watch;

namespace Fabrica.Utilities.Pipeline;

public class Pipeline<TContext> where TContext : class, IPipelineContext
{

    private readonly Func<TContext, Task> _executablePipeline;

    internal Pipeline(Func<TContext,Task> executablePipeline)
    {
        _executablePipeline = executablePipeline;
    }

    public async Task ExecuteAsync( TContext context )
    {
        
        Guard.IsNotNull(context, nameof(context));        
        
        using var logger = this.EnterMethod();
        
        Guard.IsNotNull(context, nameof(context));

        try
        {

            // *************************************************
            logger.Debug("Attempting to execute pipeline");
            await _executablePipeline(context);
            
        }
        catch (Exception cause)
        {
            logger.Error(cause, "Unhandled Exception encountered: Failed to execute pipeline");
            throw;
        }        

        
    }
    
}