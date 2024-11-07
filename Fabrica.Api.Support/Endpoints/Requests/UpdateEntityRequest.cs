using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record UpdateEntityRequest<TDelta>( [FromRoute(Name = "uid")] string Uid, [FromBody] TDelta Delta) : BaseUpdateEntityRequest<TDelta>(Uid, Delta) where TDelta : BaseDelta;
