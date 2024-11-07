using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Api.Persistence.Handlers;

public interface IQueryService
{

    ICorrelation Correlation { get; }

    IRuleSet Rules { get; }
    IMapper Mapper { get; }

    DbContext DbContext { get; }

}