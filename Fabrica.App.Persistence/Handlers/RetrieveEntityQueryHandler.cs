using CommunityToolkit.Diagnostics;
using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Fabrica.Watch;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Handlers;

public abstract class RetrieveEntityQueryHandler<TRequest, TEntity>( IQueryService service, Func<IQueryable<TEntity>,IQueryable<TEntity>>? eager=null ) : BaseOneQueryHandler<TRequest,TEntity>(service) where TRequest : BaseOneRequest<TEntity> where TEntity : class, IEntity
{

    protected virtual IQueryable<TEntity> GetEagerQueryable( IQueryable<TEntity> set ) => eager is null ? set: eager(set);


    public override async Task<Response<TEntity>> Handle( TRequest request, CancellationToken ct = default )
    {

        Guard.IsNotNull(request);
        
        using var logger = EnterMethod();

        
        // *************************************************
        var oneResult = await Service.Repository.EagerByUidAsync<TEntity>( request.Uid, GetEagerQueryable, ct );
        if (oneResult.IsError)
            return oneResult.ToResponse();

        
        // *************************************************
        logger.Debug("Attempting to call OnSuccess");
        var result = await OnSuccess( request, oneResult.AsEntity, ct );    
        
        
        // *************************************************        
        return result; 
        
        
    }


}