using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public interface ICommandService
{

    ICorrelation Correlation { get; }

    IRuleSet Rules { get; }
    IMapper Mapper { get; }

    IUnitOfWork Uow { get; }
    DbContext DbContext { get; }


}