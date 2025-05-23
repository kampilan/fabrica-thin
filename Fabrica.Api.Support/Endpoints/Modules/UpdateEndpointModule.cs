﻿using Fabrica.Api.Endpoints.Requests;
using Fabrica.Endpoints;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Endpoints.Modules;

public abstract class UpdateEndpointModule<TDelta,TEntity>(ICorrelation correlation, IRequestMediator mediator) : BaseDeltaDispatchEndpointModule<UpdateEntityRequest<TDelta>, TEntity>(correlation, mediator), IEndpointModule where TDelta: BaseDelta where TEntity : class, IEntity
{

    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var route = $"{GetRoute()}/{{uid}}";

        builder.MapPut(route, async ([AsParameters] UpdateEntityRequest<TDelta> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Update {Name}")
            .WithDescription($"Update {Name} using Uid")
            .Produces<Response>()
            .Produces<ProblemDetail>(400, "application/problem+json")
            .Produces<ProblemDetail>(404, "application/problem+json")
            .Produces<ProblemDetail>(422, "application/problem+json")
            .WithOpenApi();

    }


}