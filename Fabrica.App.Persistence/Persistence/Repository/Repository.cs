using System.Linq.Expressions;
using Fabrica.Exceptions;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Rules.Exceptions;
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

    
    public async Task<CountOrError> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
            logger.DebugFormat("Found {0} matches", count );                
    
        
            // *************************************************
            return count;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to count {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx );
        }
        
        
    }

    public async Task<ExistsOrError> ExistsAsync<TEntity>( Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default ) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();


        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to count {0} using predicate", name );
            var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
            logger.DebugFormat("Found {0} matches", count );                
        

        
            // *************************************************
            return count > 0 ;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to check existence of {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx );       
        }

    }

    public async Task<EntityOrError<TEntity>> OneByIdAsync<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to count {0} using predicate", name );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);

            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Id: ({id})");
        
        
            logger.LogObject(nameof(entity), entity);        

        
            // *************************************************        
            return entity;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Id: ({id})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }
        
    }

    public async Task<EntityOrError<TEntity>> OneByUidAsync<TEntity>( string uid, CancellationToken ct = default ) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to find {0} by Uid: ({1})", name, uid );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);    
    
            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Uid: ({uid})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Uid: ({uid})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);            
        }
        
        
    }

    public async Task<EntityOrError<TEntity>> EagerByIdAsync<TEntity>(long id, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to eagerly find {0} by Id: ({1})", name, id );
            var queryable = eager(context.Set<TEntity>());
            var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);    

            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Id: ({id})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Id: ({id})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }
        
    }

    public async Task<EntityOrError<TEntity>> EagerByUidAsync<TEntity>(string uid, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to eagerly find {0} by Uid: ({1})", name, uid );
            var queryable = eager( context.Set<TEntity>() );
            var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);    

            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Uid: ({uid})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;            

            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Uid: ({uid})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }
        
    }

    public async Task<ListOrError<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var list = await context.Set<TEntity>().Where(predicate).ToListAsync(ct);    
        
            logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
            // *************************************************
            return list;            

            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to query many {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }        

    }

    public async Task<ListOrError<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,IQueryable<TEntity>> many, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var queryable = many(context.Set<TEntity>());
            var list = await queryable.Where(predicate).ToListAsync(ct);    
        
            logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
            // *************************************************
            return list;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to query many {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }
        
        
    }
    
}




public class OriginRepository(ICorrelation correlation, DbContext context): CorrelatedObject(correlation), IOriginRepository
{

    
    public IQueryable<TEntity> Queryable<TEntity>(Func<IQueryable<TEntity>,IQueryable<TEntity>>? builder=null) where TEntity : class, IEntity
    {
        return builder is null ? context.Set<TEntity>() : builder(context.Set<TEntity>());
    }
    
    
    public async Task<CountOrError> CountAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
            logger.DebugFormat("Found {0} matches", count );                
        

        
            // *************************************************
            return count;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to count {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }        
        
        
    }

    public async Task<ExistsOrError> ExistsAsync<TEntity>( Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default ) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to count {0} using predicate", name );
            var count = await context.Set<TEntity>().CountAsync(predicate, ct);        
        
            logger.DebugFormat("Found {0} matches", count );                
        

        
            // *************************************************
            return count > 0 ;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to check existence of {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }

    }

    public async Task<EntityOrError<TEntity>> OneByIdAsync<TEntity>(long id, CancellationToken ct = default) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to count {0} using predicate", name );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);    
        
            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Id: ({id})");
        
            logger.LogObject(nameof(entity), entity);        


        
            // *************************************************        
            return entity;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Id: ({id})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }
        
    }

    public async Task<EntityOrError<TEntity>> OneByUidAsync<TEntity>( string uid, CancellationToken ct = default ) where TEntity : class, IEntity
    {
        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to find {0} by Uid: ({1})", name, uid );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);    

            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Uid: ({uid})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Uid: ({uid})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }
        
    }

    public async Task<EntityOrError<TEntity>> EagerByIdAsync<TEntity>(long id, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to eagerly find {0} by Id: ({1})", name, id );
            var queryable = eager(context.Set<TEntity>());
            var entity = await queryable.SingleOrDefaultAsync(e => e.Id == id, ct);    

            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Id: ({id})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Id: ({id})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }
        
    }

    public async Task<EntityOrError<TEntity>> EagerByUidAsync<TEntity>(string uid, Func<IQueryable<TEntity>, IQueryable<TEntity>> eager, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to eagerly find {0} by Uid: ({1})", name, uid );
            var queryable = eager( context.Set<TEntity>() );
            var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == uid, ct);    
        
            if( entity is null )
                return NotFoundError.Create($"Could not find {name} using Uid: ({uid})");
        
            logger.LogObject(nameof(entity), entity);        
        

         
            // *************************************************        
            return entity;            

            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to find {name} using given Uid: ({uid})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }        
        
    }

    public async Task<ListOrError<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default) where TEntity : class, IEntity
    {

        
        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var list = await context.Set<TEntity>().Where(predicate).ToListAsync(ct);    
        
            logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
            // *************************************************
            return list;
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to query many {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }        

    }

    public async Task<ListOrError<TEntity>> ManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>,IQueryable<TEntity>> many, CancellationToken ct = default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to query many {0}", name );
            var queryable = many(context.Set<TEntity>());
            var list = await queryable.Where(predicate).ToListAsync(ct);    
        
            logger.DebugFormat("Found {0} {1}", list.Count, name );                
        

        
            // *************************************************
            return list;

        }
        catch (Exception cause)
        {
            var ctx = $"Failed to query many {name} using given predicate";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }
        
        
        
    }

    public async Task<OkOrError> PersistAsync<TEntity>(TEntity entity, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        try
        {
            await context.AddAsync(entity, ct);
            
            return Ok.Singleton;
            
        }
        catch (Exception cause)
        {
            var ctx = "Failed to persist entity";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);
        }
        
    }

    public async Task<OkOrError> DeleteByIdAsync<TEntity>(long id, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to find {0} by Id: ({1}) for deletion", name, id );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Id == id, ct);

            if (entity is null)
                return NotFoundError.Create($"Could not find {name} using Id: ({id})");

        
            // *************************************************
            logger.Debug("Attempting to remove entity");
            context.Set<TEntity>().Remove(entity);


        
            // *************************************************        
            return Ok.Singleton;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to delete {name} using given Id: ({id})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }

    }

    public async Task<OkOrError> DeleteByUidAsync<TEntity>(string uid, CancellationToken ct=default) where TEntity : class, IEntity
    {

        using var logger = EnterMethod();

        var name = typeof(TEntity).GetConciseFullName();

        try
        {

            // *************************************************
            logger.DebugFormat("Attempting to find {0} by Uid: ({1}) for deletion", name, uid );
            var entity = await context.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == uid, ct);
            if (entity is null)
                return NotFoundError.Create($"Could not find {name} using Uid: ({uid})");


            // *************************************************
            logger.Debug("Attempting to remove entity");
            context.Set<TEntity>().Remove(entity);

            return Ok.Singleton;            
            
        }
        catch (Exception cause)
        {
            var ctx = $"Failed to delete {name} using given Uid: ({uid})";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);      
        }

    }

    public async Task<CountOrError> SaveAsync( CancellationToken ct = default )
    {

        using var logger = EnterMethod();

        try
        {

            // *************************************************
            logger.Debug("Attempting to Save pending changes");
            var count = await context.SaveChangesAsync(ct);

            logger.Inspect(nameof(count), count);



            // *************************************************        
            return count;

        }
        catch (ViolationsExistException ve)
        {
            return NotValidError.Create(ve.Violations, "Failed to save changes");
        }
        
        catch (Exception cause)
        {
            const string ctx = "Failed to save DbContext changes";
            logger.Error(cause, ctx);
            return UnhandledError.Create(cause, ctx);       
        }

        
    }
    
    
}