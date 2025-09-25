using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record QueryByRqlEntityRequest<TEntity>( [FromQuery(Name = "rql")] string Rql ) : BaseQueryEntityRequest<TEntity>( Rql ) where TEntity : class, IEntity;