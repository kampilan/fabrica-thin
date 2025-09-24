﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.App.One.Container;
using Fabrica.App.One.Hosting;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Hosting;
using Fabrica.Watch;
using Fabrica.Watch.Bridges.MicrosoftImpl;
using Fabrica.Watch.Switching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Fabrica.App.One.Bootstraps;

public abstract class GenericHostBootstrap() : CorrelatedObject(new Correlation()), IBootstrap
{


    public string ApplicationLifetimeType { get; set; } = "FabricaOne";
    public string ApplicationBaseDirectory { get; set; } = string.Empty;
    public bool AllowManualExit { get; set; } = false;


    public IConfiguration Configuration { get; set; } = null!;

    public bool QuietLogging { get; set; } = false;

    public bool RealtimeLogging { get; set; } = false;
    public List<LocalSwitchConfig> RealtimeSwitches { get; set; } = [];

    public bool RelayLogging { get; set; } = false;


    public string WatchEventStoreUri { get; set; } = string.Empty;
    public string WatchDomainName { get; set; } = string.Empty;
    public int WatchPollingDurationSecs { get; set; } = 15;


    public bool AllowAnyIp { get; set; } = false;
    public int ListeningPort { get; set; } = 8080;


    public string Environment { get; set; } = "Development";
    public string MissionName { get; set; } = string.Empty;
    public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);


    public string ApplianceId { get; set; } = string.Empty;
    public string ApplianceName { get; set; } = string.Empty;
    public string ApplianceBuild { get; set; } = string.Empty;
    public DateTime ApplianceBuildDate { get; set; } = DateTime.MinValue;
    public string ApplianceRoot { get; set; } = string.Empty;
    public DateTime ApplianceStartTime { get; set; } = DateTime.MinValue;


    public string ContentRootPathOverride { get; set; } = string.Empty;

    public string EnvironmentName { get; private set; } = string.Empty;
    public string ContentRootPath { get; private set; } = string.Empty;
    public string ApplicationName { get; private set; } = string.Empty;

    protected IHostBuilder Builder { get; set; } = null!;
    protected IMissionContext MissionContext { get; set; } = null!;

    public virtual void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();

        maker.UseQuiet();

        maker.Build();

    }

    public async Task<IAppliance> Boot()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to call OnConfigured");
        await OnConfigured();

        logger.LogObject("Bootstrap", this);



        // *****************************************************************
        logger.Debug("Attempting to create HostBuilder");
        Builder = Host.CreateDefaultBuilder();



        // *****************************************************************
        logger.Debug("Attempting to Add Host Configuration ");
        Builder.ConfigureHostConfiguration( cfb => cfb.AddConfiguration(Configuration) );



        // *****************************************************************
        logger.Debug("Attempting to Add App Configuration ");
        Builder.ConfigureAppConfiguration( cfb => cfb.AddConfiguration(Configuration) );



        // *****************************************************************
        logger.Debug("Attempting to Configure Microsoft Logging with Watch bridge");
        Builder.ConfigureLogging(lb =>
        {
            lb.UseWatch();
        });



        // *****************************************************************
        logger.DebugFormat( "Attempting to configure ApplicationLifetime{0}", ApplicationLifetimeType );
        switch (ApplicationLifetimeType.ToLowerInvariant())
        {
            case "fabricaone":
                Builder.UseFabricaOne(ApplicationBaseDirectory, AllowManualExit);
                break;
            case "systemd":
                Builder.UseSystemd();
                break;
        }



        // *****************************************************************
        logger.Debug("Attempting to call ConfigureServices");
        Builder.ConfigureServices(sc =>
        {

            sc.AddHostedService<RequiresStartService>();

        });



        // *****************************************************************
        logger.Debug("Attempting to call UseServiceProviderFactory");
        Builder.UseServiceProviderFactory(new FabricaServiceProviderFactory(cb =>
        {

            cb.RegisterInstance(Configuration)
                .As<IConfiguration>()
                .SingleInstance();


            cb.AddCorrelation();


            MissionContext = Configuration.Get<MissionContext>()??new MissionContext();
            cb.RegisterInstance(MissionContext)
                .AsSelf()
                .As<IMissionContext>()
                .SingleInstance();


            var inner = GetLogger();

            try
            {
                inner.Debug("Attempting to call Build");

                var services = new ServiceCollection();
                
                Build( Builder, services, cb );

                cb.Populate(services);

            }
            catch (Exception cause)
            {
                inner.ErrorWithContext(cause, this, "Bootstrap Build failed.");
                throw;
            }


        }));

        // *****************************************************************
        logger.Debug("Attempting to Configure WebHost");
        Builder.ConfigureWebHost(whb =>
        {

            whb.UseKestrel(op =>
            {
                if (AllowAnyIp)
                    op.ListenAnyIP(ListeningPort);
                else
                    op.ListenLocalhost(ListeningPort);

            });


            whb.Configure(app =>
            {

                var inner = GetLogger();

                try
                {

                    inner.Debug("Attempting to call BuildWebApp");

                    BuildWebApp(whb, app);

                }
                catch (Exception cause)
                {
                    inner.ErrorWithContext(cause, this, "Bootstrap BuildWebApp failed.");
                    throw;
                }

            });


            if (!string.IsNullOrWhiteSpace(ContentRootPathOverride))
                whb.UseContentRoot(ContentRootPathOverride);


        });






        // *****************************************************************
        logger.Debug("Attempting to build Host");
        var host = Builder.Build();



        // *****************************************************************
        logger.Debug("Attempting to create HostAppliance");
        var app = new HostAppliance(host);



        // *****************************************************************
        return app;


    }


    protected virtual Task OnConfigured()
    {

        using var logger = EnterMethod();

        logger.Info("Base OnConfigured does nothing");

        return Task.CompletedTask;

    }


    protected abstract void Build(IHostBuilder host, IServiceCollection services, ContainerBuilder builder);
    protected abstract void BuildWebApp(IWebHostBuilder host, IApplicationBuilder app);


}


