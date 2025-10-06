using Fabrica.Persistence.Entities;
using JetBrains.Annotations;

namespace Fabrica.Watch.Http.Models;

[UsedImplicitly]
public class DomainExplorer
{

    public string Uid { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string ServerUri { get; set; } = string.Empty;
    
}