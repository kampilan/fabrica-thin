namespace Fabrica.Utilities.Pipeline;

public interface IPipelineBuilder<TContext>  where TContext : class, IPipelineContext
{

    IPipelineBuilder<TContext> AddStep(IPipelineStep<TContext> step);

    Pipeline<TContext> Build();

}