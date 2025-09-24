using Fabrica.Exceptions;
using Fabrica.Models;
using Fabrica.Persistence;
using OneOf;

namespace Fabrica.App.Persistence.Repository;

[GenerateOneOf]
public partial class EntityOrError<TEntity>: OneOfBase<TEntity,Error> where TEntity : class, IEntity
{

    public bool IsSuccess => IsT0;
    public TEntity AsEntity => AsT0;

    public bool IsError => IsT1;
    public Error AsError => AsT1;

    public Response<TEntity> ToResponse() => Match<Response<TEntity>>(o => o, e => e);


}