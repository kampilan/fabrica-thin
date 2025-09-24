using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Persistence;
using OneOf;

namespace Fabrica.App.Persistence.Repository;

[GenerateOneOf]
public partial class OkOrError : OneOfBase<Ok, Error>
{

    public bool IsSuccess => IsT0;
    public Ok AsOk => AsT0;
    
    public bool IsError => IsT1;
    public Error AsError => AsT1;

    public Response ToResponse() => Match( _ => Error.Ok, e => e );

    public Response<TEntity> ToResponse<TEntity>() where TEntity : class, IEntity
    {
        return Match(_ => Error.Ok, e => e);
    }

}
