
namespace Fabrica.Watch;

public struct LoggerRequest()
{
    public bool Debug { get; set; } = false;
    public Level Level { get; set; } = Level.Debug;

    public string Tenant { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;


    //public IList<(string Key, string Target)> FilterKeys { get; } = new List<(string Key, string Target)>();


}