using Fabrica.Models;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public abstract record BaseCreateEntityRequest<TDelta>(TDelta Delta) : IRequest<Response> where TDelta : BaseDelta;