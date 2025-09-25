﻿using Fabrica.Api.Endpoints.Requests;
using Fabrica.Endpoints;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Endpoints.Modules;

public abstract class RetrieveEndpointModule<TEntity>(ICorrelation correlation, IRequestMediator mediator) : BaseOneDispatchEndpointModule<RetrieveEntityRequest<TEntity>, TEntity>(correlation, mediator), IEndpointModule where TEntity : class, IEntity
{

    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var route = $"{GetRoute()}/{{uid}}";

        builder.MapGet( route , async ([AsParameters] RetrieveEntityRequest<TEntity> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Retrieve {Name}")
            .WithDescription($"Retrieve {Name} using Uid")
            .Produces<TEntity>()
            .Produces<ProblemDetail>(400, "application/problem+json")
            .Produces<ProblemDetail>(404, "application/problem+json")
            .WithOpenApi();

    }


}
