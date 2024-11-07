
// ReSharper disable UnusedTypeParameter

using Fabrica.Models;
using MediatR;

namespace Fabrica.Api.Persistence.Requests;

public abstract record BaseDeleteEntityRequest<TEntity>(string Uid) : IRequest<Response>;


