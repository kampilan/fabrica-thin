﻿using Fabrica.App.One.Lifetime;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// ReSharper disable UnusedMember.Global

namespace Fabrica.App.One.Bootstraps;

public static class HostBuilderExtensions
{


    public static IHostBuilder UseFabricaOne(this IHostBuilder builder, string path = "", bool allowExit = false)
    {

        builder.ConfigureServices((_, sc) =>
        {

            sc.AddSingleton(_ => new FileSignalController(FileSignalController.OwnerType.Appliance, path));
            sc.AddSingleton<ISignalController>(sp => sp.GetRequiredService<FileSignalController>());
            sc.AddSingleton<IRequiresStart>(sp => sp.GetRequiredService<FileSignalController>());

            sc.AddSingleton(typeof(IHostLifetime), sp =>
            {

                var clo = sp.GetRequiredService<IOptions<ConsoleLifetimeOptions>>();
                var he = sp.GetRequiredService<IHostEnvironment>();
                var hal = sp.GetRequiredService<IHostApplicationLifetime>();
                var ho = sp.GetRequiredService<IOptions<HostOptions>>();

                if (allowExit)
                {
                    var sg = sp.GetRequiredService<ISignalController>();
                    return new ApplianceConsoleLifetimeWithExit(sg, clo, he, hal, ho);
                }

                return new ApplianceConsoleLifetime(clo, he, hal, ho);

            });

            sc.AddSingleton(sp =>
            {
                var hal = sp.GetRequiredService<IHostApplicationLifetime>();
                var sg = sp.GetRequiredService<ISignalController>();

                return new ApplianceLifetime(hal, sg);

            });

            sc.AddSingleton<IRequiresStart>(sp => sp.GetRequiredService<ApplianceLifetime>());

        });


        return builder;

    }

}