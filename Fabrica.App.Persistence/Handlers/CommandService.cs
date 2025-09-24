using Fabrica.App.Persistence.Repository;
using Fabrica.App.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MapsterMapper;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fabrica.App.Handlers;

public interface ICommandService
{

    ICorrelation Correlation { get; }

    IRuleSet Rules { get; }
    IMapper Mapper { get; }

    IUnitOfWork Uow { get; }
    IOriginRepository Repository { get; }

}


public class CommandService : ICommandService
{

    public required ICorrelation Correlation { get; init; }

    public required IRuleSet Rules { get; init; }
    public required IMapper Mapper { get; init; }

    public required IUnitOfWork Uow { get; init; }
    public required IOriginRepository Repository { get; init; }

}