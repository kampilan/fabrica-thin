using Fabrica.Models;
using MediatR;

namespace Fabrica.Mediator;


public class CompositeRequest
{
    public List<IRequest<Response>> Components { get; set; } = [];

}

public class CompositeRequest<TResponse> where TResponse : class
{

    public List<IRequest<Response>> Components { get; set; } = [];

    public IRequest<Response<TResponse>> Responder { get; set; } = null!;


}
