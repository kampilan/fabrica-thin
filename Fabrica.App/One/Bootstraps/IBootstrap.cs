using Fabrica.App.One.Hosting;
using Microsoft.Extensions.Configuration;

// ReSharper disable UnusedMemberInSuper.Global

namespace Fabrica.App.One.Bootstraps;

public interface IBootstrap
{

    string ApplicationBaseDirectory { get; set; }
    IConfiguration Configuration { get; set; }

    Task<IAppliance> Boot();

    void ConfigureWatch();

}