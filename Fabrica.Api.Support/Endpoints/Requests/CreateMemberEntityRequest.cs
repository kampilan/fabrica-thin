using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record CreateMemberEntityRequest<TParent,TDelta>( [FromRoute(Name = "uid")] string Uid, [FromBody] TDelta Delta ): BaseCreateMemberEntityRequest<TParent,TDelta>( Uid, Delta ) where TDelta : BaseDelta where TParent : class, IEntity;
