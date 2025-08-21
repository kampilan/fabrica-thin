namespace Fabrica.Watch;

public interface ILoggingCorrelation
{

    string CorrelationId { get; }
    string Tenant { get; }
    string Subject { get; }
    
}