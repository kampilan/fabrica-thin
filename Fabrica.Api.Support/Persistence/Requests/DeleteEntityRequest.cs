
using Fabrica.Persistence;

namespace Fabrica.Api.Persistence.Requests;

public record DeleteEntityRequest<TEntity>(string Uid) : BaseDeleteEntityRequest<TEntity>( Uid) where TEntity : class, IEntity;


