using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record CreateEntityRequest<TDelta>( [FromBody] TDelta Delta) : BaseCreateEntityRequest<TDelta>(Delta) where TDelta : BaseDelta;
