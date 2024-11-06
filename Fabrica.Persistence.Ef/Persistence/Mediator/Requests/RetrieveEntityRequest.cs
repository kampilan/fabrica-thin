
namespace Fabrica.Persistence.Mediator.Requests;

public record RetrieveEntityRequest<TEntity>(string Uid) : BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class;