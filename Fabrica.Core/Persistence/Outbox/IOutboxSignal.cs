namespace Fabrica.Persistence.Outbox;

public interface IOutboxSignal
{
    void Set();
    bool Wait(TimeSpan timeout);
}