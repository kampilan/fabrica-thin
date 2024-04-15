namespace Fabrica.Persistence;

public interface IDependentEntity: IEntity
{

    void SetParent( object? parent );

}