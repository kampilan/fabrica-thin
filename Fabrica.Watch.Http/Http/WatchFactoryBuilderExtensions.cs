using Fabrica.Watch.Http.Sink;

namespace Fabrica.Watch.Http;

public static class WatchFactoryBuilderExtensions
{


    public static RelayEventSink UseRelaySink(this WatchFactoryBuilder builder, int port = 5246, string? domain = null )
    {

        var sink = new RelayEventSink
        {
            Port = port
        };

        builder.AddSink(sink);

        return sink;

    }


    public static HttpEventSink UseHttpSink(this WatchFactoryBuilder builder, string uri, string domain, TimeSpan pollingInterval = default)
    {

        builder.PollingInterval = pollingInterval != default?pollingInterval:TimeSpan.FromMilliseconds(50);

        var sink = new HttpEventSink
        {
            WatchEndpoint = uri,
            Domain = domain
        };

        builder.AddSink(sink);

        return sink;

    }


}