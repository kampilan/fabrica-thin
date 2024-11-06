using Fabrica.Models;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers;

public abstract class UpdateEntityCommand<TRequest, TEntity, TDelta>(ICommandService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response> where TRequest : BaseUpdateEntityRequest<TDelta> where TEntity : class, IEntity, new() where TDelta : BaseDelta
{

    protected ICommandService Service { get; init; } = service;

    protected virtual Task Before(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task After(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }


    public async Task<Response> Handle(TRequest request, CancellationToken token = default)
    {

        using var logger = EnterMethod();


        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);

        var deltaTypeName = typeof(TDelta).GetConciseFullName();
        logger.Inspect(nameof(deltaTypeName), deltaTypeName);


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to create new entity");
            var entity = await Service.DbContext.Set<TEntity>().SingleOrDefaultAsync(e => e.Uid == request.Uid, token);
            if (entity is null)
                return Response.NotFound($"Could not find {entityTypeName} using Uid ({request.Uid})");


            // *****************************************************************
            logger.Debug("Attempting to call before");
            await Before(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to map delta to entity");
            Service.Mapper.Map(request.Delta, entity);



            // *****************************************************************
            logger.Debug("Attempting to call after");
            await After(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to save changes");
            var affected = await Service.DbContext.SaveChangesAsync(token);



            // *****************************************************************
            logger.Debug("Attempting to mark Uow as CanCommit");
            Service.Uow.CanCommit();



            // *****************************************************************
            return Response.Ok(entity.Uid, affected);


        }
        catch (Exception)
        {
            Service.Uow.MustRollback();
            throw;
        }


    }


}