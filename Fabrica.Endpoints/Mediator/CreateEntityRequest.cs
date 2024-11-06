using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record CreateEntityRequest<TDelta>( [FromBody] TDelta Delta) : BaseCreateEntityRequest<TDelta>(Delta) where TDelta : BaseDelta;
