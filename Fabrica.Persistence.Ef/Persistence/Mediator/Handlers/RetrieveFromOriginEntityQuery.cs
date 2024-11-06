using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers;


public abstract class RetrieveFromOriginEntityQuery<TRequest, TEntity>( ICommandService service ) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response<TEntity>> where TRequest : BaseRetrieveEntityRequest<TEntity> where TEntity : class, IEntity
{

    protected ICommandService Service { get; init; } = service;

    protected abstract Func<IQueryable<TEntity>, IQueryable<TEntity>> Eager { get; set; }


    public async Task<Response<TEntity>> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();

        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);



        // *****************************************************************
        logger.Debug("Attempting to build Eager IQueryable");
        var queryable = Eager(Service.DbContext.Set<TEntity>().AsQueryable());



        // *****************************************************************
        logger.Debug("Attempting to fetch entity by Uid");
        var entity = await queryable.SingleOrDefaultAsync(e => e.Uid == request.Uid, cancellationToken);
        if (entity is null)
            return Response<TEntity>.NotFound($"Could not find {entityTypeName} using Uid: ({request.Uid})");



        // *****************************************************************
        return entity;


    }


}