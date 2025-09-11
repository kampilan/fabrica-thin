using Fabrica.App.Requests;
using Fabrica.Persistence;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Dispatchers;

/// <summary>
/// An abstract base class for handling HTTP endpoint requests to fetch a single resource
/// identified by a unique identifier (UID). This class extends <c>BaseOneDispatcher</c>
/// and provides additional functionality by mapping the "uid" route parameter to the
/// request object.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object used to retrieve the resource. This type must inherit
/// from <c>BaseOneByUidRequest&lt;TResponse&gt;</c>.
/// </typeparam>
/// <typeparam name="TEntity">
/// The type of the response object representing the resource data retrieved.
/// </typeparam>
/// <remarks>
/// The class is designed to simplify the process of building and handling requests
/// where a resource is retrieved using its UID. It automatically assigns the "uid" route
/// parameter to the corresponding property in the request object and facilitates its handling
/// using MediatR.
/// </remarks>
public abstract class RetrieveEntityDispatcher<TRequest, TEntity>: BaseOneDispatcher<TRequest,TEntity> where TRequest : BaseOneRequest<TEntity>, new() where TEntity : class, IEntity
{

    [FromRoute(Name = "uid")] 
    public string Uid { get; set; } = string.Empty;
            
    protected override TRequest Build()
    {

        var request = new TRequest
        {
            Uid = Uid
        };

        return request;

    }
    
}

/// <summary>
/// An abstract base class for handling HTTP endpoint requests that fetch a single resource
/// using a unique identifier (UID). This class extends <c>BaseOneDispatcher</c>, providing
/// added functionality to map the "uid" route parameter to the request object and process it
/// accordingly.
/// </summary>
/// <typeparam name="TRequest">
/// The type of the request object used to retrieve the resource. This type must inherit
/// from <c>BaseOneByUidRequest&lt;TResponse&gt;</c>.
/// </typeparam>
/// <typeparam name="TEntity">
/// The type of the model or domain object representing the resource retrieved from the backend.
/// </typeparam>
/// <typeparam name="TDto">
/// The type of the DTO (Data Transfer Object) used for external representation of the resource.
/// </typeparam>
/// <remarks>
/// The class is designed to handle HTTP requests where a unique identifier ("uid")
/// is passed as a route parameter, automatically assigning it to the request object.
/// This simplifies routing and data retrieval, leveraging MediatR for request handling.
/// </remarks>
public abstract class RetrieveEntityDispatcher<TRequest,TEntity,TDto>: BaseOneDispatcher<TRequest,TEntity,TDto> where TRequest : BaseOneRequest<TEntity>, new() where TEntity : class, IEntity where TDto: class
{

    [FromRoute(Name = "uid")] 
    public string Uid { get; set; } = string.Empty;
            
    protected override TRequest Build()
    {

        var request = new TRequest
        {
            Uid = Uid
        };

        return request;

    }
    
}