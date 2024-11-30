namespace Fabrica.Watch.Utilities;

public static class WatchHelpers
{

    private static readonly DateTime Epoch = new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long ToWatchTimestamp( DateTime source = default )
    {

        if( source == default )
            source = DateTime.Now;

        return (long)(source.ToUniversalTime() - Epoch).TotalMicroseconds;

    }

    public static DateTime FromWatchTimestamp( long timestamp )
    {

        var ts = TimeSpan.FromMicroseconds(timestamp);
        return Epoch.Add(ts).ToLocalTime();

    }

}