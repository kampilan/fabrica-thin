using Fabrica.App.Requests;
using Fabrica.Persistence;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Fabrica.App.Dispatchers;

[UsedImplicitly]
public abstract class CreateEntityDispatcher<TRequest,TEntity,TDelta> : BaseOneDispatcher<TRequest,TEntity> where TRequest: BaseOneRequest<TEntity,TDelta>, new() where TEntity : class, IEntity where TDelta : BaseDelta
{

    [FromBody]
    public TDelta Delta { get; set; } = null!;


    protected override TRequest Build()
    {

        var request = new TRequest
        {
            Delta = Delta
        };

        return request;

    }

}