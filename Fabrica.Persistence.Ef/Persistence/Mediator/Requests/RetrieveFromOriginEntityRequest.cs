namespace Fabrica.Persistence.Mediator.Requests;

public record RetrieveFromOriginEntityRequest<TEntity>( string Uid ) : BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class;