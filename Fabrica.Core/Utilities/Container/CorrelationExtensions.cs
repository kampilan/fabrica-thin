﻿// ReSharper disable UnusedMember.Global

using System.Runtime.CompilerServices;
using System.Security.Claims;
using Autofac;
using Fabrica.Identity;
using Fabrica.Utilities.Types;
using Fabrica.Watch;

namespace Fabrica.Utilities.Container;

public static class CorrelationExtensions
{


    public static ContainerBuilder AddCorrelation( this ContainerBuilder builder )
    {

        builder.Register(_ => new Correlation())
            .As<ICorrelation>()
            .As<ILoggingCorrelation>()
            .AsSelf()
            .InstancePerLifetimeScope();

        return builder;

    }

    public static ContainerBuilder CloneCorrelation(this ContainerBuilder builder, ICorrelation source)
    {

        builder.Register( _=> source.Clone() )
            .As<ICorrelation>()
            .As<ILoggingCorrelation>()
            .AsSelf()
            .InstancePerLifetimeScope();

        return builder;

    }


    public static ClaimsIdentity ToIdentity(this ICorrelation correlation)
    {
      
        var identity = correlation.Caller?.Identity as ClaimsIdentity ?? new AnonymousClaimsIdentity();
        return identity;

    }

    public static bool TryGetAuthenticatedIdentity(this ICorrelation correlation, out ClaimsIdentity? ci)
    {

        var caller = correlation.Caller;

        ci = null;

        if( caller?.Identity == null || !caller.Identity.IsAuthenticated )
            return false;

        if( caller.Identity is ClaimsIdentity cand )
        {
            ci = cand;
            return true;
        }

        return false;

    }


    public static ILogger GetLogger(this ICorrelation correlation, string category)
    {

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(category));


        var subject = "";
        if( correlation.Caller is ClaimsPrincipal cp )
            subject = cp.GetUserName();


        // ******************************************************
        var request = new LoggerRequest
        {
            Tenant        = correlation.Tenant,
            Subject       = subject,
            Category      = category,
            CorrelationId = correlation.Uid
        };


        if( correlation is Correlation {Debug: true} impl )
        {
            request.Debug = true;
            request.Level = impl.Level;
        }


        //request.FilterKeys.Add(("Subject", subject));
        //request.FilterKeys.Add(("Tenant", correlation.Tenant));



        // ******************************************************
        var logger = WatchFactoryLocator.Factory.GetLogger( ref request );



        // ******************************************************
        return logger;


    }


    public static ILogger GetLogger<T>(this ICorrelation correlation)
    {

        var category = typeof(T).GetConciseFullName() ?? "Unknown";

        return GetLogger( correlation, category );

    }

    public static ILogger GetLogger(this ICorrelation correlation, Type type)
    {

        if (type == null) throw new ArgumentNullException(nameof(type));

        var category = type.GetConciseFullName() ?? "Unknown";

        return GetLogger(correlation, category);

    }


    public static ILogger GetLogger(this ICorrelation correlation, object target)
    {

        if (target == null) throw new ArgumentNullException(nameof(target));

        var category = target.GetType().GetConciseFullName() ?? "Unknown";

        return GetLogger(correlation, category);

    }


    public static ILogger EnterMethod(this ICorrelation correlation, string category, [CallerMemberName] string name = "")
    {

        var logger = GetLogger(correlation, category);
        logger.EnterMethod(name);

        return logger;

    }

    public static ILogger EnterMethod<T>(this ICorrelation correlation, [CallerMemberName] string name = "")
    {

        var logger = GetLogger<T>(correlation);
        logger.EnterMethod(name);

        return logger;

    }

    public static ILogger EnterMethod(this ICorrelation correlation, Type type, [CallerMemberName] string name = "")
    {

        var logger = GetLogger(correlation, type);
        logger.EnterMethod(name);

        return logger;

    }





}