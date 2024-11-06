using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record UpdateEntityRequest<TDelta>( [FromRoute(Name = "uid")] string Uid, [FromBody] TDelta Delta) : BaseUpdateEntityRequest<TDelta>(Uid, Delta) where TDelta : BaseDelta;
