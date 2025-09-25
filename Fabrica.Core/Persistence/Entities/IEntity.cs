namespace Fabrica.Persistence.Entities;

public interface IEntity
{

    long Id { get; }
    string Uid { get; }

    void OnCreate();
    void OnModification();


}