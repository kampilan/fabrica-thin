using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers;

public interface IQueryService
{

    ICorrelation Correlation { get; }

    DbContext DbContext { get; }

}