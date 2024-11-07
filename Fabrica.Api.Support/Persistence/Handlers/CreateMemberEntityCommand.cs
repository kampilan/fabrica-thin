using Fabrica.Api.Persistence.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public abstract class CreateMemberEntityCommand<TRequest, TParent, TEntity, TDelta>(ICommandService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response> where TRequest : BaseCreateMemberEntityRequest<TParent, TDelta> where TParent : class, IEntity where TEntity : class, IEntity, new() where TDelta : BaseDelta
{

    protected ICommandService Service { get; init; } = service;


    protected virtual Task<TEntity> CreateEntity()
    {
        var entity = new TEntity();
        return Task.FromResult(entity);
    }


    protected abstract Task Attach(TParent parent, TEntity entity);


    protected virtual Task Before(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task After(TRequest request, TEntity entity)
    {
        return Task.CompletedTask;
    }


    public async Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();


        var parentTypeName = typeof(TParent).GetConciseFullName();
        logger.Inspect(nameof(parentTypeName), parentTypeName);

        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);

        var deltaTypeName = typeof(TDelta).GetConciseFullName();
        logger.Inspect(nameof(deltaTypeName), deltaTypeName);


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to fetch parent");
            var parent = await Service.DbContext.Set<TParent>()
                .SingleOrDefaultAsync(e => e.Uid == request.ParentUid, cancellationToken);
            if (parent is null)
                return Response.NotFound($"Could not find {parentTypeName} using Uid ({request.ParentUid})");



            // *****************************************************************
            logger.Debug("Attempting to create new entity");
            var entity = await CreateEntity();



            // *****************************************************************
            logger.Debug("Attempting to call before");
            await Before(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to map delta to entity");
            Service.Mapper.Map(request.Delta, entity);



            // *****************************************************************
            logger.Debug("Attempting to persist entity");
            Service.DbContext.Add(entity);



            // *****************************************************************
            logger.Debug("Attempting to call attach");
            await Attach(parent, entity);



            // *****************************************************************
            logger.Debug("Attempting to call after");
            await After(request, entity);



            // *****************************************************************
            logger.Debug("Attempting to save changes");
            var affected = await Service.DbContext.SaveChangesAsync(cancellationToken);



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