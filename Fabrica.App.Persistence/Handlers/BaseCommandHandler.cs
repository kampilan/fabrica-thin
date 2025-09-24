using Fabrica.Models;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.App.Handlers;

public abstract class BaseCommandHandler<TRequest>(ICommandService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest,Response> where TRequest : IRequest<Response>
{


    protected ICommandService Service => service;

    public abstract Task<Response> Handle(TRequest request, CancellationToken token = default);


}