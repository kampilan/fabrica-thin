using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Rql.Serialization;
using Fabrica.Watch;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.App.Handlers;

/// <summary>
/// Represents a base class for handling many-query operations based on RQL criteria.
/// </summary>
/// <typeparam name="TRequest">The type of the request. Must inherit from <see cref="BaseManyRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the response objects.</typeparam>
public abstract class ManyEntityQueryHandler<TRequest, TEntity>( IQueryService service, Func<IQueryable<TEntity>,IQueryable<TEntity>>? many=null ) : BaseManyQueryHandler<TRequest, TEntity>(service) where TRequest : BaseManyRequest<TEntity> where TEntity : class, IEntity
{

    protected virtual IQueryable<TEntity> GetManyQueryable( IQueryable<TEntity> set ) => many is null ? set: many(set);
    
    public override async Task<Response<List<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();

        var plural = nameof(TEntity).Pluralize();
        var queryable = GetManyQueryable(Service.Repository.Queryable<TEntity>());

        
        // *************************************************
        logger.Debug("Attempting to process given criteria");
        var result = new HashSet<TEntity>();
        foreach (var c in request.Predicates )
        {

            var rql = c.ToRqlCriteria();
            var expr = c.ToExpression();
                
            var list = await queryable.Where(expr).ToListAsync(cancellationToken: cancellationToken);
            logger.DebugFormat("Rql ({0}) produced {1} {2}", rql, list.Count, plural);                

            result.UnionWith(list);
                
        }

        logger.DebugFormat("Produced {0} unique {1}", result.Count, plural );


        
        // *************************************************            
        return result.ToList();

    }
        
}