using Fabrica.Persistence.UnitOfWork;
using Fabrica.Utilities.Container;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public interface ICommandService
{

    ICorrelation Correlation { get; }

    IUnitOfWork Uow { get; }
    DbContext DbContext { get; }

    IMapper Mapper { get; }

}