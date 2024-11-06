namespace Fabrica.Persistence.Mediator.Requests;

public record RetrieveEntityFromOriginRequest<TEntity>( string Uid ) : BaseRetrieveEntityRequest<TEntity>( Uid ) where TEntity : class;