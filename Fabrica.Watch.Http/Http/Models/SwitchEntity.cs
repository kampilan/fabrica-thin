using JetBrains.Annotations;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable UnusedMember.Global

namespace Fabrica.Watch.Http.Models;

[UsedImplicitly]
public class SwitchEntity
{

    public string Uid { get; set; } = string.Empty;

    public string DomainUid { get; set; } = string.Empty;

    public string Pattern { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;


    public string FilterType { get; set; } = string.Empty;
    public string FilterTarget { get; set; } = string.Empty;


    public string Level { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;


}    
