using Fabrica.Watch.Http.Sink;
using Fabrica.Watch.Http.Switches;
using JetBrains.Annotations;

namespace Fabrica.Watch.Http;

[UsedImplicitly]
public static class WatchFactoryBuilderExtensions
{

    [UsedImplicitly]
    public static JsonHttpEventSinkProvider UseJsonHttpSink(this WatchFactoryBuilder builder, string serverUrl, string domainName, TimeSpan pollingInterval = default)
    {
        
        var sink = new JsonHttpEventSinkProvider
        {
            WatchServerUrl = serverUrl,
            DomainName = domainName
        };

        builder.Sink = sink;

        return sink;

    }

    
    [UsedImplicitly]
    public static BinaryHttpEventSinkProvider UseBinaryHttpSink(this WatchFactoryBuilder builder, string serverUrl, string domainName, TimeSpan pollingInterval = default)
    {

        var sink = new BinaryHttpEventSinkProvider
        {
            WatchServerUrl = serverUrl,
            DomainName = domainName
        };

        builder.Sink = sink;

        return sink;

    }    


    [UsedImplicitly]    
    public static WatchFactoryBuilder UseHttpSwitchSource( this WatchFactoryBuilder builder, string serverUrl, string domainName )
    {

        var source = new HttpSwitchSource
        {
            WatchServerUrl  = serverUrl,
            DomainName = domainName
        };

        builder.Source = source;

        return builder;

    }    
    

}