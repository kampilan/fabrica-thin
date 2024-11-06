
// ReSharper disable UnusedTypeParameter

using Fabrica.Models;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public abstract record BaseDeleteEntityRequest<TEntity>(string Uid) : IRequest<Response>;


