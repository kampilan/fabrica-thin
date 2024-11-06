namespace Fabrica.Persistence.Mediator.Requests;

public record CreateEntityRequest<TDelta>(TDelta Delta) : BaseCreateEntityRequest<TDelta>(Delta) where TDelta : BaseDelta;