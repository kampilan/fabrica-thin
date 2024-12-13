using Fabrica.Watch.Http.Sink;
using System;

namespace Fabrica.Watch.Http;

public static class WatchFactoryBuilderExtensions
{


    public static JsonHttpEventSinkProvider UseRelaySink(this WatchFactoryBuilder builder, int port = 5246, string? domainUid = null )
    {
        
        var sink = new JsonHttpEventSinkProvider
        {
            SinkEndpoint = $"http://localhost:{port}",
            DomainUid    = domainUid??""
        };


        builder.AddSink(sink);

        return sink;

    }


    public static JsonHttpEventSinkProvider UseHttpSink(this WatchFactoryBuilder builder, string uri, string domainUid, TimeSpan pollingInterval = default)
    {

        builder.PollingInterval = pollingInterval != default?pollingInterval:TimeSpan.FromMilliseconds(50);

        var sink = new JsonHttpEventSinkProvider
        {
            SinkEndpoint = uri,
            DomainUid    = domainUid
        };

        builder.AddSink(sink);

        return sink;

    }


}