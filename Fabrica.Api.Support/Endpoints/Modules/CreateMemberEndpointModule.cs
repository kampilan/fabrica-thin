using Fabrica.Api.Endpoints.Requests;
using Fabrica.Endpoints;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Utilities.Container;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Endpoints.Modules;

public abstract class CreateMemberEndpointModule<TParent,TDelta,TEntity>(ICorrelation correlation, IRequestMediator mediator) : BaseDeltaDispatchEndpointModule<CreateMemberEntityRequest<TParent, TDelta>, TEntity>(correlation, mediator), IEndpointModule where TParent: class, IEntity where TDelta: BaseDelta where TEntity: class, IEntity
{

    protected string ParentName => typeof(TParent).Name;
    protected string ParentPlural => ParentName.Pluralize();
    protected string ParentLower => ParentPlural.ToLowerInvariant();


    protected virtual string GetParentResourceName()
    {
        return ParentLower;
    }


    public void AddRoutes(IEndpointRouteBuilder builder)
    {

        var parent = GetParentResourceName();
        var member = GetResourceName();

        var route = $"{parent}/{{uid}}/{member}";

        builder.MapPost(route, async ([AsParameters] CreateMemberEntityRequest<TParent,TDelta> request) => await Handle(request))
            .WithTags(Plural)
            .WithSummary($"Create {Name}")
            .WithDescription($"Create {Name} from Delta")
            .Produces<Response>()
            .Produces<ProblemDetail>(400, "application/problem+json")
            .Produces<ProblemDetail>(404, "application/problem+json")
            .Produces<ProblemDetail>(422, "application/problem+json")
            .WithOpenApi();

    }


}