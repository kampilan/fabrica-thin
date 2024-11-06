using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record CreateMemberEntityRequest<TParent,TDelta>( [FromRoute(Name = "uid")] string Uid, [FromBody] TDelta Delta ): BaseCreateMemberEntityRequest<TParent,TDelta>( Uid, Delta ) where TDelta : BaseDelta where TParent : class, IEntity;
