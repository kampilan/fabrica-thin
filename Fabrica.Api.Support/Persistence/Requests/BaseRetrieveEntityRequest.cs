using Fabrica.Models;
using MediatR;

namespace Fabrica.Api.Persistence.Requests;

public abstract record BaseRetrieveEntityRequest<TEntity>( string Uid ) : IRequest<Response<TEntity>> where TEntity : class;