using System.Reflection;
using Fabrica.App.Persistence.Contexts;
using Fabrica.Exceptions;
using Fabrica.Identity;
using Fabrica.Persistence;
using Fabrica.Persistence.Audit;
using Fabrica.Persistence.Entities;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Fabrica.App.Persistence.Interceptors;

public interface IValidationTarget
{
    IRuleSet Rules { get; }
    DbContext Context { get; }
}

public interface IJournalingTarget
{
    ICorrelation Correlation { get; }
    IRootEntity? Root { get; }
    DbContext Context { get; }
}

public class OriginSaveChangesInterceptor( ICorrelation correlation ): SaveChangesInterceptor
{

    private static readonly HashSet<EntityState> DirtyStates = [EntityState.Added, EntityState.Modified];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {

        using var logger = correlation.EnterMethod<OriginSaveChangesInterceptor>();

        
        // *************************************************
        logger.Debug("Attempting to checking that DBContext is IOriginDbContext");
        if (eventData.Context is not IOriginDbContext)
            return result;


        // *************************************************
        logger.Debug("Attempting to perform callbacks");
        PerformCallbacks( eventData.Context );

        
        // *************************************************        
        if( eventData.Context is IValidationTarget vt )
        {

            logger.Debug("Attempting to perform validation");
            var er = PerformEvaluation(vt);
            if( er is { valid: false, violations: not null } && er.violations.Count > 0)
                throw new FailedValidationException(er.violations);
            
        }

        
        // *************************************************        
        if (eventData.Context is IJournalingTarget jt)
        {
            logger.Debug("Attempting to perform journaling");
            var journals = PerformEntityJournaling(jt);
            await eventData.Context.AddRangeAsync(journals, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);

    }

    
    
    private void PerformCallbacks( DbContext context )
    {
        
        using var logger = correlation.EnterMethod<OriginSaveChangesInterceptor>();

        foreach (var candidate in context.ChangeTracker.Entries().Where(e=>DirtyStates.Contains(e.State)) )
        {

            if (candidate is { State: EntityState.Added, Entity: IEntity created })
            {
                created.OnCreate();
                created.OnModification();
            }

            if (candidate is { State: EntityState.Modified, Entity: IEntity modified })
                modified.OnModification();

        }


    }        
    
    

    private(bool valid, List<EventDetail>? violations) PerformEvaluation( IValidationTarget target )
    {

        using var logger = correlation.EnterMethod<OriginSaveChangesInterceptor>();

        
        // *****************************************************************
        logger.Debug("Attempting to find all dirty entities");
        var entries = target.Context.ChangeTracker.Entries().Where(e => DirtyStates.Contains(e.State)).ToList();
        var entities = entries.Select(d => d.Entity).ToList();

        logger.Inspect(nameof(entities.Count), entities.Count);



        // *****************************************************************
        logger.Debug("Attempting to evaluate all tracked entities");            
        if( !target.Rules.TryValidate( entities, out var violations ) )
            return (false,violations);



        // *****************************************************************
        return (true,null);
        
        
    }


    private IList<AuditJournal> PerformEntityJournaling( IJournalingTarget target )
    {

        using var logger = correlation.EnterMethod<OriginSaveChangesInterceptor>();


        var journalTime = DateTime.Now;
        var journals = new List<AuditJournal>();


        if (target.Root != null)
        {

            logger.Debug("Attempting to create unmodified root journal entry");
            var aj = CreateAuditJournal(AuditJournalType.UnmodifiedRoot, target.Root);

            journals.Add(aj);
        }


        // *****************************************************************
        logger.Debug("Attempting to inspect each entity in this DbContext");
        foreach (var entry in target.Context.ChangeTracker.Entries())
        {


            if (entry.Entity is not IEntity entity)
                continue;


            var audit = entry.Entity.GetType().GetCustomAttribute<AuditAttribute>(true);


            if (audit == null || audit is { Read: false, Write: false })
                continue;



            void Log(string message = "")
            {

                if (logger.IsTraceEnabled)
                {

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        logger.Trace(message);
                    }

                    var name = entity.GetType().GetConciseName();
                    logger.LogObject(nameof(audit),
                        new { EntityType = name, audit.EntityName, audit.Read, audit.Write, audit.Detailed });

                }

            }


            // *****************************************************************                    
            if (audit.Read)
            {

                Log("Attempting to create read journal entry");

                var aj = CreateAuditJournal(AuditJournalType.Read, entity);
                journals.Add(aj);

            }



            // *****************************************************************
            if (entry.State == EntityState.Added && audit.Write)
            {

                Log("Attempting to create insert journal entry");

                var aj = CreateAuditJournal(AuditJournalType.Created, entity);
                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry);

            }



            // *****************************************************************
            if (entry.State == EntityState.Modified && audit.Write)
            {

                logger.Debug("Attempting to create update journal entry");
                Log("Attempting to create update journal entry");

                var aj = CreateAuditJournal(AuditJournalType.Updated, entity);
                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry);

            }



            // *****************************************************************
            if (entry.State == EntityState.Deleted && audit.Write)
            {

                Log("Attempting to create delete journal entry");

                var aj = CreateAuditJournal(AuditJournalType.Deleted, entity);
                journals.Add(aj);

            }



            // *****************************************************************
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entry.State == EntityState.Unchanged && audit.Write && entity is IRootEntity && target.Root == null)
            {

                Log("Attempting to create unmodified root journal entry");

                var aj = CreateAuditJournal(AuditJournalType.UnmodifiedRoot, entity);
                journals.Add(aj);

            }


        }



        // *****************************************************************        
        logger.Inspect("Journal Count", journals.Count);
        return journals;


        void PerformDetailJournaling(EntityEntry entry)
        {
            if (entry.Entity is not IEntity entity)
                return;

            journals.AddRange(from prop in entry.Properties
                where prop.IsModified
                select CreateAuditJournal(AuditJournalType.Detail, entity, prop));
        }


        AuditJournal CreateAuditJournal(AuditJournalType ajt, IEntity entity, PropertyEntry? prop = null)
        {

            var ident = target.Correlation.ToIdentity();

            var aj = new AuditJournal
            {
                TypeCode = ajt.ToString(),
                UnitOfWorkUid = target.Correlation.Uid,
                SubjectUid = ident.GetSubject("Anonymous"),
                SubjectDescription = ident.GetName(),
                Occurred = journalTime,
                Entity = entity.GetType().FullName ?? "",
                EntityUid = entity.Uid,
                EntityDescription = entity.ToString() ?? ""
            };


            if (prop != null)
            {

                aj.PropertyName = prop.Metadata.Name;

                var prev = prop.OriginalValue?.ToString() ?? "";
                if (prev.Length > 255)
                    prev = prev[..255];

                aj.PreviousValue = prev;

                var curr = prop.CurrentValue?.ToString() ?? "";
                if (curr.Length > 255)
                    curr = curr[..255];

                aj.CurrentValue = curr;

            }


            return aj;

        }


    }


}