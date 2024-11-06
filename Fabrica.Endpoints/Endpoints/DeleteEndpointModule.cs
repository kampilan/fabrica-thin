using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Routing;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Endpoints;

public abstract class DeleteEndpointModule<TEntity>(ICorrelation correlation, IRequestMediator mediator) : BaseDispatchEndpointModule<DeleteEntityRequest<TEntity>, TEntity>(correlation, mediator), IEndpointModule where TEntity : class, IEntity
{

    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var route = $"{GetRoute()}/{{uid}}";

        builder.MapDelete(route, async ([AsParameters] DeleteEntityRequest<TEntity> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Delete {Name}")
            .WithDescription($"Delete {Name} using Uid")
            .Produces<Response>()
            .Produces<ProblemDetail>(400, "application/problem+json")
            .Produces<ProblemDetail>(404, "application/problem+json")
            .WithOpenApi();

    }


}