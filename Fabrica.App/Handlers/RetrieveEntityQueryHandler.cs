using CommunityToolkit.Diagnostics;
using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
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
        var one = await Service.Repository.EagerByUidAsync<TEntity>( request.Uid, GetEagerQueryable, ct );
        if( one is null )
            return Response<TEntity>.NotFound($"Could not find {nameof(TEntity)} using Uid: ({request.Uid})");

        
        // *************************************************
        logger.Debug("Attempting to call OnSuccess");
        var result = await OnSuccess( request, one, ct );    
        
        
        // *************************************************        
        return result; 
        
        
    }


}