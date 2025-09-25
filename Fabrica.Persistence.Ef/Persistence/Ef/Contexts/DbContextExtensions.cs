using System.Reflection;
using Fabrica.Exceptions;
using Fabrica.Identity;
using Fabrica.Persistence.Audit;
using Fabrica.Persistence.Entities;
using Fabrica.Persistence.Repository;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fabrica.Persistence.Ef.Contexts;

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


[UsedImplicitly]
public static class DbContextExtensions
{

    
    [UsedImplicitly]
    public static (bool valid, List<EventDetail>? violations) PerformEvaluation(this IValidationTarget target)
    {

        var type = target.Context.GetType();        
        
        using var logger = WatchFactoryLocator.Factory.GetLogger( type );


        
        // *****************************************************************
        logger.Debug("Attempting to find all dirty entities");
        var entries = target.Context.ChangeTracker.Entries().Where(e => Helpers.DirtyStates.Contains(e.State)).ToList();
        var entities = entries.Select(d => d.Entity).ToList();

        logger.Inspect(nameof(entities.Count), entities.Count);



        // *****************************************************************
        logger.Debug("Attempting to evaluate all tracked entities");            
        if( !target.Rules.TryValidate( entities, out var violations ) )
            return (false,violations);



        // *****************************************************************
        return (true,null);
        
        
    }

    

    [UsedImplicitly]
    public static void PerformCallbacks( this DbContext context )
    {

        var type = context.GetType();        
        
        using var logger = WatchFactoryLocator.Factory.GetLogger( type );


        foreach (var candidate in context.ChangeTracker.Entries().Where(e=>Helpers.DirtyStates.Contains(e.State)) )
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
    
    [UsedImplicitly]
    public static IList<AuditJournal> PerformEntityJournaling( this IJournalingTarget target )
    {

        var type = target.Context.GetType();        
        
        using var logger = WatchFactoryLocator.Factory.GetLogger( type );


        var journalTime = DateTime.Now;
        var journals = new List<AuditJournal>();


        if (target.Root != null)
        {

            logger.Debug("Attempting to create unmodified root journal entry");
            var aj = CreateAuditJournal( AuditJournalType.UnmodifiedRoot, target.Root );

            journals.Add(aj);
        }


        // *****************************************************************
        logger.Debug("Attempting to inspect each entity in this DbContext");
        foreach (var entry in target.Context.ChangeTracker.Entries() )
        {


            if (entry.Entity is not IEntity entity)
                continue;


            var audit = entry.Entity.GetType().GetCustomAttribute<AuditAttribute>(true);


            if (audit == null || audit is { Read: false, Write: false })
                continue;



            void Log( string message="" )
            {

                if( logger.IsTraceEnabled )
                {

                    if( string.IsNullOrWhiteSpace(message) )
                    {
                        logger.Trace(message);
                    }

                    var name = entity.GetType().GetConciseName();
                    logger.LogObject(nameof(audit), new { EntityType = name, audit.EntityName, audit.Read, audit.Write, audit.Detailed });

                }

            }


            // *****************************************************************                    
            if (audit.Read)
            {

                Log("Attempting to create read journal entry");

                var aj = CreateAuditJournal( AuditJournalType.Read, entity);
                journals.Add(aj);

            }



            // *****************************************************************
            if (entry.State == EntityState.Added && audit.Write)
            {

                Log("Attempting to create insert journal entry");

                var aj = CreateAuditJournal( AuditJournalType.Created, entity);
                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry);

            }



            // *****************************************************************
            if (entry.State == EntityState.Modified && audit.Write)
            {

                logger.Debug("Attempting to create update journal entry");
                Log( "Attempting to create update journal entry" );

                var aj = CreateAuditJournal( AuditJournalType.Updated, entity);
                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry);

            }



            // *****************************************************************
            if (entry.State == EntityState.Deleted && audit.Write)
            {

                Log("Attempting to create delete journal entry");

                var aj = CreateAuditJournal( AuditJournalType.Deleted, entity );
                journals.Add(aj);

            }



            // *****************************************************************
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (entry.State == EntityState.Unchanged && audit.Write && entity is IRootEntity && target.Root == null)
            {

                Log("Attempting to create unmodified root journal entry");

                var aj = CreateAuditJournal( AuditJournalType.UnmodifiedRoot, entity );
                journals.Add(aj);

            }


        }



        // *****************************************************************        
        logger.Inspect("Journal Count", journals.Count);
        return journals;


        void PerformDetailJournaling( EntityEntry entry )
        {
            if (entry.Entity is not IEntity entity)
                return;

            journals.AddRange(from prop in entry.Properties where prop.IsModified select CreateAuditJournal( AuditJournalType.Detail, entity, prop) );
        }        
        

        AuditJournal CreateAuditJournal( AuditJournalType ajt, IEntity entity, PropertyEntry? prop = null )
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