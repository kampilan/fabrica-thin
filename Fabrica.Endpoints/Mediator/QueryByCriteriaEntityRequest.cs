using Fabrica.Persistence;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Mediator;

public record QueryByCriteriaEntityRequest<TCriteria,TEntity>( [FromBody] TCriteria Criteria ) : BaseQueryEntityRequest<TCriteria,TEntity>( Criteria ) where TCriteria: BaseCriteria where TEntity : class, IEntity;
