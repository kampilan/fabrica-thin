
namespace Fabrica.Persistence.Mediator.Requests;

public record DeleteEntityRequest<TEntity>(string Uid) : BaseDeleteEntityRequest<TEntity>( Uid) where TEntity : class, IEntity;


