using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Persistence;
using OneOf;

namespace Fabrica.App.Persistence.Repository;

[GenerateOneOf]
public partial class ListOrError<TEntity>: OneOfBase<List<TEntity>,Error> where TEntity: class, IEntity
{

    public bool IsSuccess => IsT0;
    public List<TEntity> AsList => AsT0;

    public bool IsError => IsT1;
    public Error AsError => AsT1;


    public Response<List<TEntity>> ToResponse() => Match<Response<List<TEntity>>>(o => o, e => e);


}