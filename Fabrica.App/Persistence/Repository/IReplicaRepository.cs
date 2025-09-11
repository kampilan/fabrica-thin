using System.Linq.Expressions;
using Fabrica.Persistence;

namespace Fabrica.App.Persistence.Repository;

public interface IReplicaRepository
{

    IQueryable<TEntity> Queryable<TEntity>( Func<IQueryable<TEntity>,IQueryable<TEntity>>? builder=null ) where TEntity : class, IEntity;

    Task<int> CountAsync<TEntity>(Expression<Func<TEntity,bool>> predicate,CancellationToken cancellationToken = default) where TEntity : class, IEntity;
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity,bool>> predicate,CancellationToken cancellationToken = default) where TEntity : class, IEntity;    
    
    Task<TEntity?> OneByIdAsync<TEntity>( long id, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;
    Task<TEntity?> OneByUidAsync<TEntity>( string uid, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;
    
    Task<TEntity?> EagerByIdAsync<TEntity>( long id, Func<IQueryable<TEntity>,IQueryable<TEntity>> eager, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;
    Task<TEntity?> EagerByUidAsync<TEntity>( string uid, Func<IQueryable<TEntity>,IQueryable<TEntity>> eager, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;

    Task<List<TEntity>> ManyAsync<TEntity>( Expression<Func<TEntity,bool>> predicate, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;
    Task<List<TEntity>> ManyAsync<TEntity>( Expression<Func<TEntity,bool>> predicate, Func<IQueryable<TEntity>,IQueryable<TEntity>> many, CancellationToken cancellationToken = default ) where TEntity : class, IEntity;
    
}