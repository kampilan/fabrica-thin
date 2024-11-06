using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record DeleteEntityRequest<TEntity>( [FromRoute(Name = "uid")] string Uid ) : BaseDeleteEntityRequest<TEntity>( Uid ) where TEntity : class, IEntity;
