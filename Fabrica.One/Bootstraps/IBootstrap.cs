using Fabrica.One;
using Microsoft.Extensions.Configuration;

// ReSharper disable UnusedMemberInSuper.Global

namespace Fabrica.Bootstraps;

public interface IBootstrap
{

    string ApplicationBaseDirectory { get; set; }
    IConfiguration Configuration { get; set; }

    Task<IAppliance> Boot();

    void ConfigureWatch();

}