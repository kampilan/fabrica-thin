namespace Fabrica.Watch;

public interface ILoggingCorrelation
{

    string Uid { get; }
    string Tenant { get; }
    string Subject { get; }
    
}