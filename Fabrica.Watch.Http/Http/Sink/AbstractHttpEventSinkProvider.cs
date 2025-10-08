using System.Net.Http.Json;
using CommunityToolkit.Diagnostics;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Watch.Http.Models;
using Fabrica.Watch.Http.Switches;
using Fabrica.Watch.Sink;
using Flurl;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Fabrica.Watch.Http.Sink;

public abstract class AbstractHttpEventSinkProvider: IEventSinkProvider
{

    public string WatchServerUrl { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;

    private ConsoleEventSink DebugSink { get; } = new();

    protected abstract Task<HttpContent> BuildContentAsync(LogEventBatch batch);


    private ServiceProvider? _provider;    
    private string _domainUid = "";
    public async Task Start()
    {

        Guard.IsNotNullOrWhiteSpace(WatchServerUrl);
        Guard.IsNotNullOrWhiteSpace(DomainName);

        
        // *************************************************
        try
        {

            var uri = new Uri(WatchServerUrl);
            Guard.IsTrue(uri.IsAbsoluteUri, "ServerUrl must be an absolute path");
            
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();
            logger.Error( cause, "Failed to validate Url: ({0})", WatchServerUrl );
        }


        // *******************************************************        
        var services = new ServiceCollection();
        services
            .AddHttpClient( "Watch", c =>
            {
                c.BaseAddress = new Uri(WatchServerUrl);
            })
            .AddStandardResilienceHandler(o =>
            {
                o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
                o.Retry.MaxRetryAttempts = 2;
            });
        
        _provider = services.BuildServiceProvider();


        
        // *************************************************        
        try
        {

            var factory = _provider!.GetRequiredService<IHttpClientFactory>();
            using var client = factory.CreateClient("Watch");
            
            var rql = RqlFilterBuilder<DomainEntity>
                .Where(e => e.Name).Equals(DomainName)
                .ToRqlCriteria();

            var url = new Url()
                .AppendPathSegment("domains")
                .AppendQueryParam("rql", rql, true);

            var list = await client.GetFromJsonAsync<List<DomainExplorer>>(url);

            if (list is not null && list.Count > 0)
            {
                var domain = list.First();
                _domainUid = domain.Uid;
            }
            
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger(GetType());
            logger.Error(cause, "Failed to fetch Domain using Name: ({0}) from Url: ({1})", DomainName, WatchServerUrl);
        }            


    }

    public Task Stop()
    {
        return Task.CompletedTask;

    }
  
    
    public async Task Accept( LogEventBatch batch, CancellationToken cancellationToken=default )
    {

        if( cancellationToken.IsCancellationRequested )
            return;
        
        try
        {


            // *****************************************************************            
            if( string.IsNullOrWhiteSpace(batch.DomainUid) )
                batch.DomainUid = _domainUid;


            
            // *****************************************************************
            var content = await BuildContentAsync(batch);


            
            // *******************************************************            
            var factory = _provider!.GetRequiredService<IHttpClientFactory>();
            using var client = factory.CreateClient("Watch");


            
            // *****************************************************************            
            var url = new Url().AppendPathSegment("sink");

            await client.PostAsync(url, content, cancellationToken);

        }
        catch (Exception cause )
        {
            var logger = DebugSink.GetLogger( GetType());
            logger.Error(cause, "Failed to accept batch");
        }


    }


}