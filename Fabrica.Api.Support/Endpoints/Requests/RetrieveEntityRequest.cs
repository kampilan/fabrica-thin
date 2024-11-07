using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record RetrieveEntityRequest<TEntity>( [FromRoute(Name="uid")] string Uid ): BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class, IEntity;
