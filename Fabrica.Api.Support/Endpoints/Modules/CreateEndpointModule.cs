using Fabrica.Api.Endpoints.Requests;
using Fabrica.Endpoints;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Endpoints.Modules;

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
