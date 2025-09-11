using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable ClassNeverInstantiated.Global

namespace Fabrica.App.Persistence.Contexts;

public class ReplicaDbContextOptionsBuilder(ICorrelation correlation, ILoggerFactory factory) : DbContextOptionsBuilder
{
    public ICorrelation Correlation { get; } = correlation;
    public ILoggerFactory LoggerFactory { get; } = factory;
}