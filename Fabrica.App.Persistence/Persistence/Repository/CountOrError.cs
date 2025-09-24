using Fabrica.Exceptions;
using OneOf;

namespace Fabrica.App.Persistence.Repository;

[GenerateOneOf]
public partial class CountOrError : OneOfBase<int, Error>
{

    public bool IsSuccess => IsT0;
    public int Count => AsT0;

    public bool IsError => IsT1;
    public Error AsError => AsT1;


}