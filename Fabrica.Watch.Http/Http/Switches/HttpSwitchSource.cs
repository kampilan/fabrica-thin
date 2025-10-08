using System.Drawing;
using System.Net.Http.Json;
using CommunityToolkit.Diagnostics;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Watch.Http.Models;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using Flurl;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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

    
    public string WatchServerUrl { get; set; } = string.Empty;
    public HttpSwitchSource WithWatchServerUrl( string url )
    {

        Guard.IsNotNullOrWhiteSpace(url, nameof(url));

        WatchServerUrl = url;

        return this;
        
    }


    public string DomainName { get; set; } = "";
    public HttpSwitchSource WithDomainName( string domainName )
    {

        Guard.IsNotNullOrWhiteSpace(domainName, nameof(domainName));

        DomainName = domainName;

        return this;
        
    }

    private ConsoleEventSink DebugSink { get; } = new ();
    
    
    private ServiceProvider? _provider;    

    
    private readonly SemaphoreSlim _cycleLock = new (1, 1);
    private bool Started { get; set; }
    public override async Task Start()
    {

        
        Guard.IsNotNullOrWhiteSpace(WatchServerUrl);
        Guard.IsNotNullOrWhiteSpace(DomainName);

        
        // *************************************************
        try
        {

            var uri = new Uri(WatchServerUrl);
            Guard.IsTrue(uri.IsAbsoluteUri, "WatchServerUrl must be an absolute path");
            
        }
        catch (Exception cause)
        {
            var logger = DebugSink.GetLogger<HttpSwitchSource>();
            logger.Error( cause, "Failed to validate Url: ({0})", WatchServerUrl );
            return;
        }


        
        // *************************************************        
        try
        {

            var acquired = await _cycleLock.WaitAsync(TimeSpan.FromMilliseconds(50));
            if( !acquired || Started )
                return;

            
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
        
            {

            
                // *******************************************************            
                var factory = _provider!.GetRequiredService<IHttpClientFactory>();
                using var client = factory.CreateClient("Watch");

            
                // *******************************************************
                try
                {

                    var rql = RqlFilterBuilder<DomainExplorer>
                        .Where(e=>e.Name).Equals(DomainName)
                        .ToRqlCriteria();

                    var url = new Url()
                        .AppendPathSegment("domains")
                        .AppendQueryParam("rql", rql, true);

                    var list = await client.GetFromJsonAsync<List<DomainExplorer>>(url).ConfigureAwait(false);
            
                    if( list is not null && list.Count > 0 )
                        _domainUid = list.First().Uid;

                }
                catch (Exception cause)
                {
                    var logger = DebugSink.GetLogger<HttpSwitchSource>();
                    logger.Error( cause, "Failed to fetch Domain using Name: ({0}) from Url: ({1})", DomainName, WatchServerUrl );
                }            
            
            }            
        
            Started = true;

            await base.Start();            

            await UpdateAsync();
            
            
        }
        finally
        {
            _cycleLock.Release();
        }

        
    }

    public override async Task Stop()
    {

        try
        {

            var acquired = await _cycleLock.WaitAsync(TimeSpan.FromMilliseconds(50));
            if( !acquired || !Started )
                return;

            if( _provider is not null )
                await _provider.DisposeAsync();
        
            await base.Stop();

            Started = false;            
            
            
        }
        finally
        {
            _cycleLock.Release();            
        }

        
    }

    
    private string? _domainUid;
    private List<SwitchDef> _switches = [];
    public override async Task UpdateAsync( CancellationToken cancellationToken=default )
    {

        
        if( cancellationToken.IsCancellationRequested )
            return;
        
        
        // *******************************************************
        DomainEntity? domain = null;        
        {

            // *******************************************************            
            var factory = _provider!.GetRequiredService<IHttpClientFactory>();
            using var client = factory.CreateClient("Watch");

            // *******************************************************
            try
            {
            
                var url = new Url()
                    .AppendPathSegment( "domains" )
                    .AppendPathSegment( _domainUid );
                
                domain = await client.GetFromJsonAsync<DomainEntity>(url, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            }
            catch( Exception cause)
            {
                var logger = DebugSink.GetLogger<HttpSwitchSource>();            
                logger.Error( cause, "Failed to fetch Domain" );
            }            
            
            
        }


        
        // *******************************************************
        if( domain is null )
        {
            Update(_switches);
            return;
        }
        

        
        // *******************************************************
        _switches = [];
        
        foreach( var entity in domain.Switches )
        {


            // *******************************************************************
            if( !(Enum.TryParse( entity.Level, true, out Level level)) )
                level = Level.Warning;


            
            // *******************************************************************            
            var color = Color.White;
            try
            {
                color = ColorTranslator.FromHtml(entity.Color);
            }
            catch
            {
                // ignore
            }


            
            // *******************************************************************            
            var def = new SwitchDef
            {
                Pattern      = entity.Pattern,
                Tag          = entity.Tag,
                FilterType   = entity.FilterType,
                FilterTarget = entity.FilterTarget,
                Level        = level,
                Color        = color
            };

            _switches.Add(def);

        }


        Update( _switches );        

    }
    
    
}