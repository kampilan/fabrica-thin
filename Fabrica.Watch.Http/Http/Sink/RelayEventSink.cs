using System.Net.Http.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Fabrica.Watch.Http.Sink;

public class RelayEventSink : IEventSinkProvider
{

    public int Port { get; set; } = 5246;

    private IContainer Container { get; set; } = null!;
    private IHttpClientFactory Factory { get; set; } = null!;

    private ConsoleEventSink DebugSink { get; } = new();

    private DateTime _pauseUntil = DateTime.MinValue;


    public Task Start()
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

        return Task.CompletedTask;

    }

    public Task Stop()
    {

        Container.Dispose();

        return Task.CompletedTask;

    }


    public async Task Accept( LogEventBatch batch )
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
                Level = (int)Level.Debug,
                Title = cause.Message,
                Error = cause
            };

            await DebugSink.Accept( LogEventBatch.Single(le) );


        }


    }

}