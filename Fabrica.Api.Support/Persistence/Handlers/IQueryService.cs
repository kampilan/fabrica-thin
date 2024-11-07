using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public interface IQueryService
{

    ICorrelation Correlation { get; }

    DbContext DbContext { get; }

}