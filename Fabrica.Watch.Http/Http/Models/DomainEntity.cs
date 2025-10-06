using System.Collections.ObjectModel;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fabrica.Watch.Http.Models;

[UsedImplicitly]
public class DomainEntity
{

    public string Uid { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public long NonDebugTimeToLiveSeconds { get; init; }
    public long DebugTimeToLiveSeconds { get; init; }

    public string ServerUri { get; init; } = string.Empty;

    public Collection<SwitchEntity> Switches { get; set; } = [];
        
}