

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using Fabrica.Models;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.App.Handlers;

public abstract class BaseHandler<TRequest,TResponse>(ICorrelation correlation): CorrelatedObject(correlation), IRequestHandler<TRequest,TResponse> where TRequest : IRequest<TResponse> where TResponse : class
{

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);


}

public abstract class BaseHandler<TRequest>(ICorrelation correlation) : CorrelatedObject(correlation), IRequestHandler<TRequest, Response> where TRequest : IRequest<Response>
{

    public abstract Task<Response> Handle(TRequest request, CancellationToken cancellationToken);

}

