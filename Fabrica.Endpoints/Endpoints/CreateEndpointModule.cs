
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Routing;
using Fabrica.Models;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Endpoints;

public abstract class CreateEndpointModule<TDelta,TEntity>( ICorrelation correlation, IRequestMediator mediator ) : BaseDeltaDispatchEndpointModule<CreateEntityRequest<TDelta>,TEntity>( correlation, mediator ), IEndpointModule where TDelta : BaseDelta where TEntity : class, IEntity
{

    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var route = GetRoute();

        builder.MapPost(route, async ([AsParameters] CreateEntityRequest<TDelta> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Create {Name}")
            .WithDescription($"Create {Name} from Delta")
            .Produces<Response>()
            .Produces<ProblemDetail>( 400, "application/problem+json" )
            .Produces<ProblemDetail>( 422, "application/problem+json" )
            .WithOpenApi();

    }


}
