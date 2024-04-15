using Fabrica.Exceptions;
using Fabrica.Models;
using OneOf;

namespace Fabrica.Persistence.Repository;

[GenerateOneOf]
public partial class OkOrError : OneOfBase<Ok, Error>
{

    public bool IsSuccess => IsT0;
    public Ok AsOk => AsT0;
    
    public bool IsError => IsT1;
    public Error AsError => AsT1;

    public Response ToResponse() => Match( _ => Response.Ok(), e => e );


}
