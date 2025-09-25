using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using MediatR;

namespace Fabrica.Api.Persistence.Requests;

public abstract record BaseCreateEntityRequest<TDelta>(TDelta Delta) : IRequest<Response> where TDelta : BaseDelta;