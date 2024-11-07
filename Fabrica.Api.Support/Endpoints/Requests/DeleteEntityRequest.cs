using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record DeleteEntityRequest<TEntity>( [FromRoute(Name = "uid")] string Uid ) : BaseDeleteEntityRequest<TEntity>( Uid ) where TEntity : class, IEntity;
