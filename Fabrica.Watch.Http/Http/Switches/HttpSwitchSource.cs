using System.Drawing;
using CommunityToolkit.Diagnostics;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Watch.Http.Models;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using Flurl;
using Flurl.Http;
using JetBrains.Annotations;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Fabrica.Watch.Http.Switches;


[UsedImplicitly]
public class HttpSwitchSource: SwitchSource
{

    
    public string ServerUrl { get; set; } = string.Empty;
    public HttpSwitchSource WithWatchServerUrl( string url )
    {

        Guard.IsNotNullOrWhiteSpace(url, nameof(url));

        ServerUrl = url;

        return this;
        
    }


    public string DomainName { get; set; } = "";
    public HttpSwitchSource WithDomainName( string domainName )
    {

        Guard.IsNotNullOrWhiteSpace(domainName, nameof(domainName));

        DomainName = domainName;

        return this;
        
    }

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(15);
    public HttpSwitchSource WithPollingInterval(TimeSpan interval)
    {
        PollingInterval = interval;
        return this;
    }
    
    public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);
    public HttpSwitchSource WithWaitForStopInterval(TimeSpan interval)
    {
        WaitForStopInterval = interval;
        return this;
    }    
    
    private EventWaitHandle MustStop { get; } = new (false, EventResetMode.ManualReset);
    private EventWaitHandle Stopped { get; } = new (false, EventResetMode.ManualReset);

    private ConsoleEventSink DebugSink { get; } = new ();
    
    private bool Started { get; set; }
    public override async Task Start()
    {
        
        Guard.IsNotNullOrWhiteSpace(ServerUrl);
        Guard.IsNotNullOrWhiteSpace(DomainName);

        if( Started )
            return;
        
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
            return;
        }

        
        
        // *************************************************        
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run( _process );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        
        Started = true;

        await base.Start();
        
    }

    public override async Task Stop()
    {

        MustStop.Set();

        Stopped.WaitOne( WaitForStopInterval );
        
        await base.Stop();

    }


    private async Task _process()
    {

        
        // *******************************************************
        try
        {

            var rql = RqlFilterBuilder<DomainEntity>
                .Where(e=>e.Name).Equals(DomainName)
                .ToRqlCriteria();                 
            
            var list = await ServerUrl
                .AppendPathSegment("domains")
                .AppendQueryParam("rql", rql, true)
                .GetJsonAsync<List<DomainExplorer>>()
                .ConfigureAwait(false);
            
            if( list is not null && list.Count > 0 )
                _domainUid = list.First().Uid;

        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();
            logger.Error( cause, "Failed to fetch Domain using Name: ({0}) from Url: ({1})", DomainName, ServerUrl );
        }


        
        // *******************************************************        
        try
        {
            await Fetch();
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();
            logger.Error( cause, "Failed to fetch initial Switches" );
        }
        
        

        // *******************************************************
        while( !MustStop.WaitOne(PollingInterval) )
        {

            try
            {
                await Fetch();           
            }
            catch
            {
                // ignore
            }
            
        }

        Stopped.Set();

    }

    
    private string? _domainUid;
    private List<SwitchDef> _switches = [];

    private async Task Fetch()
    {
        
        // *******************************************************
        DomainEntity? domain = null;
        try
        {

            domain = await ServerUrl
                .AppendPathSegment( "domains" )
                .AppendPathSegment( _domainUid )
                .GetJsonAsync<DomainEntity>();
            
        }
        catch( Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();            
            logger.Error( cause, "Failed to fetch Domain" );
        }
        

        
        // *******************************************************
        if( domain is null )
        {
            Update(_switches);
            return;
        }
        

        
        // *******************************************************
        _switches = [];
        
        foreach( var se in domain.Switches )
        {


            if ( !(Enum.TryParse( se.Level, true, out Level lv)) )
                lv = Level.Warning;


            var color = Color.White;
            try
            {
                color = Color.FromName(se.Color);
            }
            catch
            {
                // ignore
            }


            var sw = new SwitchDef
            {
                Pattern      = se.Pattern,
                Tag          = se.Tag,
                FilterType   = se.FilterType,
                FilterTarget = se.FilterTarget,
                Level        = lv,
                Color        = color
            };

            _switches.Add(sw);

        }


        Update( _switches );        

    }
    
    
}