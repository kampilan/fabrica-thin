﻿using Fabrica.Persistence.Connection;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fabrica.Persistence.Ef.Contexts;

public class OriginDbContextOptionsBuilder(ICorrelation correlation, IRuleSet rules, IConnectionResolver resolver, IUnitOfWork uow, ILoggerFactory loggerFactory) : DbContextOptionsBuilder
{
    public ICorrelation Correlation { get; } = correlation;
    public IRuleSet Rules { get; } = rules;
    public IConnectionResolver Resolver { get; } = resolver;
    public IUnitOfWork Uow { get; } = uow;
    public ILoggerFactory LoggerFactory { get; } = loggerFactory;


    public void ConfigureDbContext( DbContext context )
    {
        context.Database.UseTransaction(Uow.Transaction);
    }


}