using Fabrica.App.Requests;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Dispatchers;

public class UpdateEntityDispatcher<TRequest,TEntity,TDelta> : BaseOneDispatcher<TRequest,TEntity> where TRequest: BaseOneRequest<TEntity,TDelta>, new() where TEntity : class, IEntity where TDelta : BaseDelta
{
    
    [FromRoute(Name = "uid")] 
    public string Uid { get; set; } = string.Empty;
    
    [FromBody]
    public TDelta Delta { get; set; } = null!;
    
    protected override TRequest Build()
    {

        var request = new TRequest
        {
            Uid = Uid,
            Delta = Delta
        };

        return request;

    }
    
}