using Fabrica.Models;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers;

public abstract class CriteriaEntityQuery<TRequest,TCriteria,TEntity>(IQueryService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response<IEnumerable<TEntity>>> where TRequest : BaseQueryEntityRequest<TCriteria,TEntity> where TCriteria: BaseCriteria where TEntity : class, IEntity
{

    protected IQueryService Service { get; init; } = service;

    protected abstract Func<IQueryable<TEntity>, IQueryable<TEntity>> Many { get; set; }


    public async Task<Response<IEnumerable<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();

        var criteriaTypeName = typeof(TCriteria).GetConciseFullName();
        logger.Inspect(nameof(criteriaTypeName), criteriaTypeName);

        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);



        // *****************************************************************
        logger.Debug("Attempting to build Expression from Criteria");
        var builder = RqlFilterBuilder<TEntity>.Create();
        builder.Introspect(request.Criteria);
        var exp = builder.ToExpression();



        // *****************************************************************
        logger.Debug("Attempting to build Many IQueryable");
        var queryable = Many(Service.DbContext.Set<TEntity>().AsQueryable());



        // *****************************************************************
        logger.Debug("Attempting to fetch many entities");
        var many = await queryable.Where(exp).ToListAsync(cancellationToken);



        // *****************************************************************
        return many;


    }


}