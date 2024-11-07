using Fabrica.Models;
using Fabrica.Persistence;
using MediatR;

namespace Fabrica.Api.Persistence.Requests;

public abstract record BaseUpdateEntityRequest<TDelta>( string Uid, TDelta Delta ) : IRequest<Response> where TDelta : BaseDelta;