namespace Fabrica.Persistence.UnitOfWork;

public interface IUnitOfWorkCommitSignal
{
    void Set();
    bool Wait(TimeSpan timeout);
}