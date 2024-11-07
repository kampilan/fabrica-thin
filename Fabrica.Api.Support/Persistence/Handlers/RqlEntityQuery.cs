using Fabrica.Api.Persistence.Requests;
using Fabrica.Models;
using Fabrica.Persistence;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Rql.Serialization;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;


public abstract class RqlEntityQuery<TRequest, TEntity>(IQueryService service) : CorrelatedObject(service.Correlation), IRequestHandler<TRequest, Response<IEnumerable<TEntity>>> where TRequest : BaseQueryEntityRequest<TEntity> where TEntity : class, IEntity
{

    protected IQueryService Service { get; init; } = service;

    protected abstract Func<IQueryable<TEntity>, IQueryable<TEntity>> Many { get; set; }


    public async Task<Response<IEnumerable<TEntity>>> Handle(TRequest request, CancellationToken cancellationToken)
    {

        using var logger = EnterMethod();

        var entityTypeName = typeof(TEntity).GetConciseFullName();
        logger.Inspect(nameof(entityTypeName), entityTypeName);



        // *****************************************************************
        logger.Debug("Attempting to parse RQL");
        var tree = RqlLanguageParser.ToCriteria(request.Rql);



        // *****************************************************************
        logger.Debug("Attempting to create builder from RQL AST");
        var builder = new RqlFilterBuilder<TEntity>(tree);
        var exp = builder.ToExpression();




        // *****************************************************************
        logger.Debug("Attempting to build Many IQueryable");
        var queryable = Many( Service.DbContext.Set<TEntity>().AsQueryable() );



        // *****************************************************************
        logger.Debug("Attempting to fetch many entities");
        var many = await queryable.Where(exp).ToListAsync(cancellationToken);



        // *****************************************************************
        return many;


    }


}