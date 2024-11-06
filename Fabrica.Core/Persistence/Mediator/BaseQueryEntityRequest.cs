using Fabrica.Models;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public abstract record BaseQueryEntityRequest<TCriteria, TEntity>(TCriteria Criteria) : IRequest<Response<IEnumerable<TEntity>>> where TCriteria : BaseCriteria where TEntity : class;

public abstract record BaseQueryEntityRequest<TEntity>(string Rql) : IRequest<Response<IEnumerable<TEntity>>> where TEntity : class;