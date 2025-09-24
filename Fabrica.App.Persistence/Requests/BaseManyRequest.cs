using Fabrica.Models;
using Fabrica.Rql;
using MediatR;

namespace Fabrica.App.Requests;

public abstract class BaseManyRequest<TResponse> : AbstractRequest, IRequest<Response<List<TResponse>>> where TResponse : class
{
   
    public List<IRqlFilter<TResponse>> Predicates { get;  } = [];
    
}