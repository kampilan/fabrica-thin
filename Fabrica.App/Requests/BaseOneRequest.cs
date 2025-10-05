using Fabrica.Models;
using Fabrica.Persistence.Entities;
using MediatR;

namespace Fabrica.App.Requests;

public abstract class BaseOneRequest<TResponse> : AbstractRequest, IRequest<Response<TResponse>> where TResponse : class
{

    public string Uid { get; set; } = string.Empty;
    
}

public abstract class BaseOneRequest<TResponse,TDelta> : AbstractRequest, IRequest<Response<TResponse>> where TResponse : class where TDelta : BaseDelta
{

    public string Uid { get; set; } = string.Empty;
    public TDelta Delta { get; set; } = null!;
    
}

public abstract class BaseOneRequest<TParent,TEntity,TDelta> : AbstractRequest, IRequest<Response<TEntity>> where TParent: class, IEntity where TEntity : class, IDependentEntity where TDelta : BaseDelta
{

    public string Uid { get; set; } = string.Empty;
    public TDelta Delta { get; set; } = null!;
    
}