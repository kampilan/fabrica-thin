using Fabrica.Models;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.App.Handlers;

public abstract class BaseQueryHandler<TRequest>(IQueryService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response> where TRequest : IRequest<Response>
{


    protected IQueryService Service => service;


    public abstract Task<Response> Handle(TRequest request, CancellationToken token = default);


}



