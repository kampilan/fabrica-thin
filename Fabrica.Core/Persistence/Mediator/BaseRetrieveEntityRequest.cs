using Fabrica.Models;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public abstract record BaseRetrieveEntityRequest<TEntity>( string Uid ) : IRequest<Response<TEntity>> where TEntity : class;