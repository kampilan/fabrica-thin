using Fabrica.App.Persistence.Repository;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MapsterMapper;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fabrica.App.Handlers;



public interface IQueryService
{

    ICorrelation Correlation { get; }
    IUnitOfWork Uow { get; }
    IRuleSet Rules { get; }
    IMapper Mapper { get; }

    IReplicaRepository Repository { get; }

}

public class QueryService : IQueryService
{

    public required ICorrelation Correlation { get; init; }
    public required IUnitOfWork Uow { get; init; }
    public required IRuleSet Rules { get; init; }
    public required IMapper Mapper { get; init; }

    public required IReplicaRepository Repository { get; init; }

}