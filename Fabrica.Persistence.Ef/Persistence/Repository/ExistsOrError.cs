using Fabrica.Exceptions;
using OneOf;

namespace Fabrica.Persistence.Repository;


[GenerateOneOf]
public partial class ExistsOrError : OneOfBase<bool, Error>
{

    public bool IsSuccess => IsT0;
    public bool Exists => AsT0;

    public bool IsError => IsT1;
    public Error AsError => AsT1;


}