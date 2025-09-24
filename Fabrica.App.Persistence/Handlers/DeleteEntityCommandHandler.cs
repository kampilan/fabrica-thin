using Fabrica.App.Requests;
using Fabrica.Models;
using Fabrica.Persistence;

namespace Fabrica.App.Handlers;

public abstract class DeleteEntityCommandHandler<TRequest,TEntity>(ICommandService service) : BaseCommandHandler<TRequest>(service) where TRequest: BaseRequest where TEntity : class, IEntity
{
    
    public override async Task<Response> Handle( TRequest request, CancellationToken ct = default )
    {

        using var logger = EnterMethod();

        
        // *****************************************************************
        await Service.Repository.DeleteByUidAsync<TEntity>(request.Uid, ct);

            
        // *****************************************************************
        await Service.Repository.SaveAsync(ct);

        
        // *****************************************************************
        return Response.Ok();        
        
    }
    
}