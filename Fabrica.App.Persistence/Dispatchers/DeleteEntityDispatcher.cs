using Fabrica.App.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.App.Dispatchers;

public class DeleteEntityDispatcher<TRequest>: BaseDispatcher<TRequest> where TRequest: BaseRequest, new()
{

    [FromRoute(Name="uid")]
    public string Uid { get; set; } = string.Empty;
    
    protected override TRequest Build()
    {
        return new TRequest { Uid = Uid };
    }
    
}