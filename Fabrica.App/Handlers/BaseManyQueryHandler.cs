using Fabrica.Models;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.App.Handlers;

public abstract class BaseManyQueryHandler<TRequest, TResponse>( IQueryService service ) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response<List<TResponse>>> where TRequest : IRequest<Response<List<TResponse>>> where TResponse : class
{

    protected IQueryService Service => service;


    public abstract Task<Response<List<TResponse>>> Handle(TRequest request, CancellationToken cancellationToken);


}