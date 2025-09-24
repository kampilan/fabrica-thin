using Fabrica.App.Requests;
using Fabrica.Persistence;
using Fabrica.Rql.Builder;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Dispatchers;


[UsedImplicitly]
public class ManyByCriteriaDispatcher<TRequest, TCriteria,TResponse>: BaseManyDispatcher<TRequest,TResponse> where TRequest : BaseManyRequest<TResponse>, new() where TResponse : class where TCriteria : class, ICriteria
{

    [FromBody]
    public TCriteria Criteria { get; set; } = null!;
            
    protected override TRequest Build()
    {

        var builder = RqlFilterBuilder<TResponse>.Create();
        builder.Introspect(Criteria);
        
        var request = new TRequest();
        request.Predicates.Add(builder);

        return request;

    }

}


[UsedImplicitly]
public class ManyByCriteriaDispatcher<TRequest,TCriteria,TResponse,TDto>: BaseManyDispatcher<TRequest,TResponse,TDto> where TRequest : BaseManyRequest<TResponse>, new() where TResponse : class where TCriteria : class, ICriteria where TDto: class
{

    [FromBody]
    public TCriteria Criteria { get; set; } = null!;
            
    protected override TRequest Build()
    {

        var builder = RqlFilterBuilder<TResponse>.Create();
        builder.Introspect(Criteria);
        
        var request = new TRequest();
        request.Predicates.Add(builder);

        return request;

    }

}