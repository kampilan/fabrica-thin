
namespace Fabrica.Api.Persistence.Requests;

public record RetrieveEntityRequest<TEntity>(string Uid) : BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class;