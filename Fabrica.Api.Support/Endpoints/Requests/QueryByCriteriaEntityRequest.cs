using Fabrica.Api.Persistence.Requests;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Endpoints.Requests;

public record QueryByCriteriaEntityRequest<TCriteria,TEntity>( [FromBody] TCriteria Criteria ) : BaseQueryEntityRequest<TCriteria,TEntity>( Criteria ) where TCriteria: BaseCriteria where TEntity : class, IEntity;
