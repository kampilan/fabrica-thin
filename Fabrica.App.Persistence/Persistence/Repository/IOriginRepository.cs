using Fabrica.Persistence;
using Fabrica.Persistence.Entities;

namespace Fabrica.App.Persistence.Repository;

public interface IOriginRepository: IReplicaRepository
{

    Task<OkOrError> PersistAsync<TEntity>( TEntity entity, CancellationToken ct = default ) where TEntity : class, IEntity;
    
    Task<OkOrError> DeleteByIdAsync<TEntity>( long id, CancellationToken ct=default ) where TEntity : class, IEntity;    
    Task<OkOrError> DeleteByUidAsync<TEntity>( string uid, CancellationToken ct=default ) where TEntity : class, IEntity;

    Task<CountOrError> SaveAsync( CancellationToken ct = default );

}
