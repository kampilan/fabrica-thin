using Fabrica.Persistence;

namespace Fabrica.App.Persistence.Repository;

public interface IOriginRepository: IReplicaRepository
{

    Task PersistAsync<TEntity>( TEntity entity, CancellationToken ct = default ) where TEntity : class, IEntity;
    
    Task DeleteByIdAsync<TEntity>( long id, CancellationToken ct=default ) where TEntity : class, IEntity;    
    Task DeleteByUidAsync<TEntity>( string uid, CancellationToken ct=default ) where TEntity : class, IEntity;

    Task<int> SaveAsync( CancellationToken ct = default );

}
