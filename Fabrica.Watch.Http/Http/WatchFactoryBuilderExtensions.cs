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

        builder.UseHttpSwitchSource(serverUrl, domainName);
        
        builder.PollingInterval = pollingInterval != TimeSpan.Zero?pollingInterval:TimeSpan.FromMilliseconds(50);
        
        var sink = new JsonHttpEventSinkProvider
        {
            ServerUrl = serverUrl,
            DomainName = domainName
        };

        builder.AddSink(sink);

        return sink;

    }

    
    [UsedImplicitly]
    public static BinaryHttpEventSinkProvider UseBinaryHttpSink(this WatchFactoryBuilder builder, string serverUrl, string domainName, TimeSpan pollingInterval = default)
    {

        builder.UseHttpSwitchSource(serverUrl, domainName);        
        
        builder.PollingInterval = pollingInterval != TimeSpan.Zero?pollingInterval:TimeSpan.FromMilliseconds(50);

        var sink = new BinaryHttpEventSinkProvider
        {
            ServerUrl = serverUrl,
            DomainName = domainName
        };

        builder.AddSink(sink);

        return sink;

    }    


    [UsedImplicitly]    
    public static WatchFactoryBuilder UseHttpSwitchSource( this WatchFactoryBuilder builder, string serverUrl, string domainName )
    {

        var source = new HttpSwitchSource
        {
            ServerUrl  = serverUrl,
            DomainName = domainName
        };

        builder.Source = source;

        return builder;

    }    
    

}