using Fabrica.Persistence;
using Fabrica.Persistence.Entities;

namespace Fabrica.Api.Persistence.Requests;

public record CreateEntityRequest<TDelta>(TDelta Delta) : BaseCreateEntityRequest<TDelta>(Delta) where TDelta : BaseDelta;