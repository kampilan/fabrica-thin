using Fabrica.App.Requests;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Fabrica.App.Dispatchers;


/// <summary>
/// Provides a base implementation for dispatching requests that involve handling multiple resources with RQL (Resource Query Language) filtering capabilities.
/// This class is specifically designed to handle scenarios where the RQL query parameters are included in the request.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object. It must inherit from <see cref="BaseManyRequest{TResponse}"/> and have a parameterless constructor.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response object. It must be a class.
/// </typeparam>
public class ManyByRqlDispatcher<TRequest,TResponse>: BaseManyDispatcher<TRequest,TResponse> where TRequest : BaseManyRequest<TResponse>, new() where TResponse : class
{

    [FromQuery(Name = "rql")] 
    public string[] Rql { get; set; } = [];
            
    protected override TRequest Build()
    {
        
        var request = new TRequest();
        
        foreach (var rql in Rql)
        {

            var tree = RqlLanguageParser.ToCriteria(rql);
            var builder = new RqlFilterBuilder<TResponse>(tree);

            request.Predicates.Add(builder);
            
        }

        return request;

    }

}

/// <summary>
/// Provides a base implementation for dispatching requests that involve handling multiple resources with RQL (Resource Query Language) filtering capabilities.
/// This class supports the inclusion of RQL query parameters in the request and facilitates mapping between data transfer objects and response objects.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object. It must inherit from <see cref="BaseManyRequest{TResponse}"/> and have a parameterless constructor.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response object. It must be a class.
/// </typeparam>
/// <typeparam name="TDto">
/// The type of the data transfer object (DTO). It must be a class.
/// </typeparam>
[UsedImplicitly]
public class ManyByRqlDispatcher<TRequest,TResponse,TDto>: BaseManyDispatcher<TRequest,TResponse, TDto> where TRequest : BaseManyRequest<TResponse>, new() where TResponse : class where TDto: class
{

    [FromQuery(Name = "rql")] 
    public string[] Rql { get; set; } = [];
            
    protected override TRequest Build()
    {

        var request = new TRequest();
        
        foreach (var rql in Rql)
        {

            var tree = RqlLanguageParser.ToCriteria(rql);
            var builder = new RqlFilterBuilder<TResponse>(tree);

            request.Predicates.Add(builder);
            
        }

        return request;

    }

}

