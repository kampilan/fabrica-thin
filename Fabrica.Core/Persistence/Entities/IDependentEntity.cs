namespace Fabrica.Persistence.Entities;

public interface IDependentEntity: IEntity
{

    void SetParent( object? parent );

}