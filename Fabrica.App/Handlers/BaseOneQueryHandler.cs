using Fabrica.Models;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.App.Handlers;

public abstract class BaseOneQueryHandler<TRequest, TEntity>( IQueryService service ) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response<TEntity>> where TRequest : IRequest<Response<TEntity>> where TEntity : class
{

    protected IQueryService Service => service;

    public abstract Task<Response<TEntity>> Handle(TRequest request, CancellationToken token=default);

    protected virtual Task<Response<TEntity>> OnSuccess( TRequest request,  TEntity entity, CancellationToken ct = default )
    {
        return Task.FromResult(Response<TEntity>.Ok(entity));
    }    
    
}