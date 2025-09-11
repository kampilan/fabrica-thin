using System.Linq.Expressions;
using Fabrica.Persistence;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.Persistence.Repository;





public class ReplicaRepository( ICorrelation correlation,  DbContext context ) : CorrelatedObject(correlation), IReplicaRepository
{

    
    public IQueryable<TEntity> Queryable<TEntity>(Func<IQueryable<TEntity>,IQueryable<TEntity>>? builder=null) where TEntity : class, IEntity
    {
        return builder is null ? context.Set<TEntity>() : builder(context.Set<TEntity>());
    }

    
    public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
        logger.DebugFormat("Found {0} matches", count );                
        

        
        // *************************************************
        return count;
        
        
    }

    public async Task<bool> ExistsAsync<TEntity>( Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default ) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to count {0} using predicate", name );
        var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
        logger.DebugFormat("Found {0} matches", count );                
        

        
        // *************************************************
        return count >0 ;

    }

    public async Task<TEntity?> OneByIdAsync<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        // *************************************************
        logger.DebugFormat("Attempting to count {0} using predicate", name );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);    
        
        logger.LogObject(nameof(entity), entity);        


        
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> OneByUidAsync<TEntity>( string uid, CancellationToken ct = default ) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        


        
        // *************************************************
        logger.DebugFormat("Attempting to find {0} by Uid: ({1})", name, uid );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);    
                
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> EagerByIdAsync<TEntity>(long id, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to eagerly find {0} by Id: ({1})", name, id );
        var queryable = eager(context.Set<TEntity>());
        var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);    
        
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> EagerByUidAsync<TEntity>(string uid, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to eagerly find {0} by Uid: ({1})", name, uid );
        var queryable = eager( context.Set<TEntity>() );
        var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);    
        
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<List<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var list = await context.Set<TEntity>().Where(predicate).ToListAsync(ct);    
        
        logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
        // *************************************************
        return list;

    }

    public async Task<List<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,IQueryable<TEntity>> many, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var queryable = many(context.Set<TEntity>());
        var list = await queryable.Where(predicate).ToListAsync(ct);    
        
        logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
        // *************************************************
        return list;
        
        
    }
    
}




public class OriginRepository(ICorrelation correlation, DbContext context): CorrelatedObject(correlation), IOriginRepository
{

    
    public IQueryable<TEntity> Queryable<TEntity>(Func<IQueryable<TEntity>,IQueryable<TEntity>>? builder=null) where TEntity : class, IEntity
    {
        return builder is null ? context.Set<TEntity>() : builder(context.Set<TEntity>());
    }
    
    
    public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
        logger.DebugFormat("Found {0} matches", count );                
        

        
        // *************************************************
        return count;
        
        
    }

    public async Task<bool> ExistsAsync<TEntity>( Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default ) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to count {0} using predicate", name );
        var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
        logger.DebugFormat("Found {0} matches", count );                
        

        
        // *************************************************
        return count > 0 ;

    }

    public async Task<TEntity?> OneByIdAsync<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        // *************************************************
        logger.DebugFormat("Attempting to count {0} using predicate", name );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);    
        
        logger.LogObject(nameof(entity), entity);        


        
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> OneByUidAsync<TEntity>( string uid, CancellationToken ct = default ) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        


        
        // *************************************************
        logger.DebugFormat("Attempting to find {0} by Uid: ({1})", name, uid );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);    
                
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> EagerByIdAsync<TEntity>(long id, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to eagerly find {0} by Id: ({1})", name, id );
        var queryable = eager(context.Set<TEntity>());
        var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);    
        
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<TEntity?> EagerByUidAsync<TEntity>(string uid, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to eagerly find {0} by Uid: ({1})", name, uid );
        var queryable = eager( context.Set<TEntity>() );
        var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);    
        
        logger.LogObject(nameof(entity), entity);        
        

         
        // *************************************************        
        return entity;
        
    }

    public async Task<List<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var list = await context.Set<TEntity>().Where(predicate).ToListAsync(ct);    
        
        logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
        // *************************************************
        return list;

    }

    public async Task<List<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,IQueryable<TEntity>> many, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        

        
        
        // *************************************************
        logger.DebugFormat("Attempting to query many {0}", name );
        var queryable = many(context.Set<TEntity>());
        var list = await queryable.Where(predicate).ToListAsync(ct);    
        
        logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
        // *************************************************
        return list;
        
        
    }

    public async Task PersistAsync<TEntity>(TEntity entity, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();
        
        await context.AddAsync(entity, ct);
        
    }

    public async Task DeleteByIdAsync<TEntity>(long id, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        
    
        
        // *************************************************
        logger.DebugFormat("Attempting to find {0} by Id: ({1}) for deletion", name, id );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);
        if (entity is null)
            return;

        context.Set<TEntity>().Remove(entity);
        
    }

    public async Task DeleteByUidAsync<TEntity>(string uid, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();        
    
        
        // *************************************************
        logger.DebugFormat("Attempting to find {0} by Uid: ({1}) for deletion", name, uid );
        var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);
        if (entity is null)
            return;

        context.Set<TEntity>().Remove(entity);
        
    }

    public async Task<int> SaveAsync( CancellationToken ct = default )
    {

        using var logger = EnterMethod();


        
        // *************************************************
        logger.Debug("Attempting to Save pending changes");
        var count = await context.SaveChangesAsync(ct);

        logger.Inspect(nameof(count), count);
        

        
        // *************************************************        
        return count;
        
    }
    
    
}