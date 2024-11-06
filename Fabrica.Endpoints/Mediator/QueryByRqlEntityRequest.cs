using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record QueryByRqlEntityRequest<TEntity>( [FromQuery(Name = "rql")] string Rql ) : BaseQueryEntityRequest<TEntity>( Rql ) where TEntity : class, IEntity;