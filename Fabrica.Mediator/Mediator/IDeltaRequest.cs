using Fabrica.Persistence;

namespace Fabrica.Mediator;

public interface IDeltaRequest<out TDelta> where TDelta : BaseDelta
{
    TDelta Delta { get; }
}
