using CommunityToolkit.Diagnostics;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Watch.Http.Models;
using Fabrica.Watch.Http.Switches;
using Fabrica.Watch.Sink;
using Flurl;
using Flurl.Http;
// ReSharper disable PropertyCanBeMadeInitOnly.Global


// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Fabrica.Watch.Http.Sink;

public abstract class AbstractHttpEventSinkProvider: IEventSinkProvider
{

    public string ServerUrl { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;

    private ConsoleEventSink DebugSink { get; } = new();

    protected abstract Task<HttpContent> BuildContentAsync(LogEventBatch batch);


    private string _endpoint = "";
    private string _domainUid = "";
    public async Task Start()
    {

        Guard.IsNotNullOrWhiteSpace(ServerUrl);
        Guard.IsNotNullOrWhiteSpace(DomainName);

        
        // *************************************************
        try
        {

            var uri = new Uri(ServerUrl);
            Guard.IsTrue(uri.IsAbsoluteUri, "ServerUrl must be an absolute path");
            
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();
            logger.Error( cause, "Failed to validate Url: ({0})", ServerUrl );
        }


        
        // *************************************************        
        try
        {

            
            var rql = RqlFilterBuilder<DomainEntity>
                .Where(e=>e.Name).Equals(DomainName)
                .ToRqlCriteria();            
            
            var list = await ServerUrl
                .AppendPathSegment("domains")
                .AppendQueryParam("rql", rql, true)
                .GetJsonAsync<List<DomainExplorer>>();

            if( list is not null && list.Count > 0 )
            {
                var domain = list.First();
                _endpoint = "http://localhost:8181";
                _domainUid = domain.Uid;
            }

            
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger(GetType());
            logger.Error( cause, "Failed to fetch Domain using Name: ({0}) from Url: ({1})", DomainName, ServerUrl );
        }        


    }

    public Task Stop()
    {
        return Task.CompletedTask;

    }
  
    
    public async Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {


        try
        {


            // *****************************************************************            
            if( string.IsNullOrWhiteSpace(batch.DomainUid) )
                batch.DomainUid = _domainUid;


            
            // *****************************************************************
            var content = await BuildContentAsync(batch);

            
            // *****************************************************************            
            await _endpoint
                .AppendPathSegment("sink")
                .PostAsync( content, cancellationToken: ct );



        }
        catch (Exception cause )
        {
            var logger = DebugSink.GetLogger( GetType());
            logger.Error(cause, "Failed to accept batch");
        }


    }


}