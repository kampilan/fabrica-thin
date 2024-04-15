using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Persistence.Ef.Contexts;

public class ReplicaDbContextOptionsBuilder(ICorrelation correlation, ILoggerFactory factory) : DbContextOptionsBuilder
{
    public ICorrelation Correlation { get; } = correlation;
    public ILoggerFactory LoggerFactory { get; } = factory;
}