namespace Fabrica.Api.Persistence.Requests;

public record RetrieveFromOriginEntityRequest<TEntity>( string Uid ) : BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class;