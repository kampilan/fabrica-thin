using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.App.Persistence.Contexts;

public class NonRelDbContextOptionsBuilder(ICorrelation correlation, IRuleSet rules, ILoggerFactory loggerFactory) : DbContextOptionsBuilder
{

    public ICorrelation Correlation { get; } = correlation;
    public IRuleSet Rules { get; } = rules;
    public ILoggerFactory LoggerFactory { get; } = loggerFactory;

}