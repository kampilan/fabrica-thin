using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;

namespace Fabrica.Watch;

public class WatchFactoryConfig
{

    public bool Quiet { get; init; }

    public bool UseAutoUpdate { get; init; } = false;
    
    public int InitialPoolSize { get; init; } = 50;
    public int MaxPoolSize { get; init; } = 500;

    public int ChannelCapacity { get; init; } = 1000;
    public int BatchSize { get; init; } = 500;

    public required ISwitchSource Switches { get; init; }

    public required IEventSinkProvider Sink { get; init; }


}