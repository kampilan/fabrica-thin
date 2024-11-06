using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record RetrieveEntityRequest<TEntity>( [FromRoute(Name="uid")] string Uid ): BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class, IEntity;
