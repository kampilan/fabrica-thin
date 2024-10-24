﻿using System.Net.Http.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Fabrica.Watch.Http.Sink;

public class RelayEventSink : IEventSink
{

    public int Port { get; set; } = 5246;

    private IContainer Container { get; set; }
    private IHttpClientFactory Factory { get; set; }

    private ConsoleEventSink DebugSink { get; } = new();

    private DateTime _pauseUntil = DateTime.MinValue;


    public void Start()
    {

        var builder = new ContainerBuilder();


        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 2, fastFirst: true);
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);


        var services = new ServiceCollection();

        services.AddHttpClient("Fabrica.Watch.Relay", c =>
            {
                c.BaseAddress = new Uri($"http://localhost:{Port}/");
            })
            .AddPolicyHandler(retry);


        builder.Populate(services);

        Container = builder.Build();

        Factory = Container.Resolve<IHttpClientFactory>();

    }

    public void Stop()
    {
        Container.Dispose();
    }

    public async Task Accept(ILogEvent logEvent)
    {

        var batch = new[] { logEvent };

        await Accept(batch);

    }

    public async Task Accept(IEnumerable<ILogEvent> batch)
    {

        try
        {

            if( _pauseUntil > DateTime.Now )
                return;

            using var client = Factory.CreateClient("Fabrica.Watch.Relay");

            var response = await client.PostAsJsonAsync( "", batch );

            if( !response.IsSuccessStatusCode )
            {
                _pauseUntil = DateTime.Now.AddSeconds(60);
            }            


        }
        catch (Exception cause)
        {

            _pauseUntil = DateTime.Now.AddSeconds(60);

            var le = new LogEvent
            {
                Category = GetType().FullName??"",
                Level = Level.Debug,
                Title = cause.Message,
                Error = cause
            };

            await DebugSink.Accept(le);


        }


    }

}