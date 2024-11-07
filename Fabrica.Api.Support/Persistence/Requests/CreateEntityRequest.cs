using Fabrica.Persistence;

namespace Fabrica.Api.Persistence.Requests;

public record CreateEntityRequest<TDelta>(TDelta Delta) : BaseCreateEntityRequest<TDelta>(Delta) where TDelta : BaseDelta;