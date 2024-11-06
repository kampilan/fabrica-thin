using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Persistence;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Endpoints;

public abstract class QueryByCriteriaEndpointModule<TCriteria,TEntity>(ICorrelation correlation, IRequestMediator mediator) : BaseManyDispatchEndpointModule<QueryByCriteriaEntityRequest<TCriteria,TEntity>, TEntity>(correlation, mediator), IEndpointModule where TCriteria: BaseCriteria where TEntity : class, IEntity
{

    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var route = GetRoute();

        builder.MapGet(route, async ([AsParameters] QueryByCriteriaEntityRequest<TCriteria,TEntity> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Query {Name}")
            .WithDescription($"Query {Name} using Criteria")
            .Produces<List<TEntity>>()
            .Produces<ProblemDetail>(400, "application/problem+json")
            .Produces<ProblemDetail>(422, "application/problem+json")
            .WithOpenApi();

    }


}
