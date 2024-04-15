namespace Fabrica.Persistence;

public interface IEntity
{

    long Id { get; }
    string Uid { get; }

    void OnCreate();
    void OnModification();


}