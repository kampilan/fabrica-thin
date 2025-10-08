/*
The MIT License (MIT)

Copyright (c) 2025 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Fabrica.Watch.Pool;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using Fabrica.Watch.Utilities;
using System.Threading.Channels;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Local

namespace Fabrica.Watch;

public sealed class WatchFactory( WatchFactoryConfig config ) : IWatchFactory
{

    private static readonly ILogger Silencer = new QuietLogger();
    
    private IWatchObjectSerializer ForObject { get; set; } = new JsonWatchObjectSerializer();
    private IWatchExceptionSerializer ForException { get; set; } = new TextExceptionSerializer();


    private int InitialPoolSize { get;  } = config.InitialPoolSize;
    private int MaxPoolSize { get; } = config.MaxPoolSize;
    private Pool<Logger> LoggerPool { get; set; } = null!;
    private Pool<LogEvent> EventPool { get; set; } = null!;

    private WatchFactoryConfig Config { get; } = config;
    
    public ISwitchSource Switches { get; private set; } = config.Switches;    
    public IEventSinkProvider Sink { get; private set; } = config.Sink;
    
    private bool Quiet { get; } = config.Quiet;



    private void _return(Logger lg)
    {
         LoggerPool.Return(lg);
    }

    private void _return(LogEvent le)
    {
        EventPool.Return(le);
    }

    private WatchFactoryUpdater? _updater;
    
    private bool Started { get; set; }
    public async Task StartAsync()
    {

        if (Started)
            return;


        LoggerPool = new Pool<Logger>(() =>
        {

            var lg = new Logger(_return);
            return lg;

        }, MaxPoolSize);
        LoggerPool.Warm(InitialPoolSize);


        EventPool = new Pool<LogEvent>(() =>
        {
            var le = new LogEvent();
            le.OnDispose = _return;
            return le;

        }, MaxPoolSize);
        EventPool.Warm(InitialPoolSize);


        await Switches.Start();
        await Sink.Start();

        if (Config.UseAutoUpdate)
        {
            _updater = new WatchFactoryUpdater();
            _updater.Start();       
        }
        
        Started = true;

    }


    public async Task StopAsync()
    {

        
        try
        {
            _updater?.Stop();
            _updater = null;       
        }
        catch
        {
            // ignored            
        }
        
        
        try
        {
            _channel.Writer.Complete();
            await FlushEventsAsync(TimeSpan.Zero);
        }
        catch
        {
            // ignored            
        }
        
        try
        {
            
            await Switches.Stop();
            Switches = null!;
            
        }
        catch
        {
            //ignored
        }

        try
        {
            await Sink.Stop();
        }
        catch
        {
            //ignored
        }

        try
        {

            EventPool.Clear();
            EventPool = null!;

            LoggerPool.Clear();
            LoggerPool = null!;

        }
        catch
        {
            // ignored
        }

        
        Started = false;

    }

    private readonly Channel<LogEvent> _channel = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(config.ChannelCapacity)
    {
        
        FullMode = BoundedChannelFullMode.DropOldest,
        
        SingleReader = true,
        SingleWriter = false,
        
    });    
    
    public void Accept(LogEvent logEvent)
    {

        _channel.Writer.TryWrite(logEvent);
        
    }

    public async Task FlushEventsAsync( TimeSpan waitInterval=default, CancellationToken cancellationToken=default )
    {

        if( cancellationToken.IsCancellationRequested )
            return;

        
        // *************************************************************        
        if( waitInterval > TimeSpan.Zero )
        {

            var waitSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            waitSource.CancelAfter(waitInterval);

            try
            {
                var has = await _channel.Reader.WaitToReadAsync(waitSource.Token).ConfigureAwait(false);
                if( !has )
                    return;
            }
            catch( OperationCanceledException )
            {
                return;
            }
            catch( Exception cause )
            {
                using var logger = WatchFactoryLocator.ConsoleFactory.GetLogger<WatchFactory>();
                logger.Error(cause, "Failed to WaitToReadAsync");
                return;           
            }

        }

        
        // *************************************************************
        var batch = new LogEventBatch();
        try
        {

            while( _channel.Reader.TryRead(out var le) )
            {

                batch.Events.Add(le);

                if( batch.Events.Count >= Config.BatchSize )
                    await Send().ConfigureAwait(false);
            
            }        
        
            if( batch.Events.Count > 0 )
                await Send().ConfigureAwait(false);
            
        }
        catch (Exception cause)
        {
            using var logger = WatchFactoryLocator.ConsoleFactory.GetLogger<WatchFactory>();
            logger.Error(cause, "Failed to Process Log Events");
        }


        
        // *************************************************************
        // *************************************************************        
        async Task Send()
        {

            try
            {
                await Sink.Accept(batch, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception cause)
            {
                using var logger = WatchFactoryLocator.ConsoleFactory.GetLogger<WatchFactory>();
                logger.Error(cause, "Failed to process batch");
            }

            batch.Events.ForEach(e => e.Dispose());
            batch.Events.Clear();
            
        }

        
    }

    public async Task UpdateSwitchesAsync(CancellationToken cancellationToken = default)
    {

        if( cancellationToken.IsCancellationRequested )
            return;
        
        try
        {
            await Switches.UpdateAsync(cancellationToken);
        }
        catch (Exception cause)
        {
            using var logger = WatchFactoryLocator.ConsoleFactory.GetLogger<WatchFactory>();
            logger.Error(cause, "Failed to Update Switches");   
        }

    }


    public bool IsTraceEnabled(string category)
    {

        if( Quiet )
            return false;

        var sw = Switches.Lookup( category );

        return sw.Level is Level.Trace;

    }

    public bool IsDebugEnabled(string category)
    {

        if( Quiet )
            return false;

        var sw = Switches.Lookup( category );

        return sw.Level is Level.Debug;
        
    }

    public bool IsTraceEnabled(Type type)
    {
        return IsTraceEnabled(type.GetConciseFullName());
    }

    public bool IsDebugEnabled(Type type)
    {
        return IsDebugEnabled(type.GetConciseFullName());
    }

    public bool IsTraceEnabled<T>()
    {
        return IsTraceEnabled(typeof(T).GetConciseFullName());
    }

    public bool IsDebugEnabled<T>()
    {
        return IsDebugEnabled(typeof(T).GetConciseFullName());
    }


    public ILogger GetLogger( string category, bool retroOn=true )
    {

        // Overall Quiet
        if( Quiet )
            return Silencer;


        // Get Switch for a given Category
        var sw = Switches.Lookup( category );


        // Return the silencer if the switch level is quiet
        if (sw.Level is Level.Quiet)
            return Silencer;


        // Acquire a new logger from the pool
        var logger = LoggerPool.Acquire(0);



        // Config the acquired logger
        logger.Config( this, string.Empty, string.Empty, sw.Tag, category, string.Empty, sw.Level, sw.Color );


        // **************************************
        return logger;

    }


    public ILogger GetLogger<T>( bool retroOn = true )
    {

        var logger = GetLogger( typeof(T).GetConciseFullName(), retroOn );

        return logger;

    }



    public ILogger GetLogger( Type type, bool retroOn = true)
    {

        var logger = GetLogger(type.GetConciseFullName(), retroOn );

        return logger;

    }



    public ILogger GetLogger( ref LoggerRequest request, bool retroOn = true )
    {


        // Overall Quiet
        if (Quiet)
            return Silencer;


        // Check for the special case of the diagnostic probe
        ISwitch sw;
        if( request.Debug )
        {
            var color = Switches.LookupColor(request.Category);
            sw = new Switch {Level = request.Level, Color = color, Pattern = string.Empty, Tag = "Diagnostics"};
        }
        else
            sw = Switches.Lookup( request.Category );



        // Return the silencer if the switch level is quiet
        if ( sw.Level is Level.Quiet )
            return Silencer;


        // Acquire a new logger from the pool
        var logger = LoggerPool.Acquire(0);


        // Config the acquired logger
        logger.Config( this, request.Tenant, request.Subject, sw.Tag, request.Category, request.CorrelationId, sw.Level, sw.Color );


        // ************************************************************
        return logger;


    }

    public LogEvent AcquireLogEvent()
    {

        var le =  EventPool.Acquire(0);
        le.Reset();

        return le;

    }

    public void ReturnLogEvent( LogEvent le ) => EventPool.Return(le);

    public void Enrich( LogEvent logEvent )
    {

        if( !string.IsNullOrWhiteSpace(logEvent.Payload) )
            return;

        if( logEvent.Error is not null )
        {
            var (type, source) = ForException.Serialize(logEvent.Error, logEvent.ErrorContext);
            logEvent.Type = (int)type;
            logEvent.Payload = source;
        }
        else if( logEvent.Object is not null )
        {
            var (type, source) = ForObject.Serialize(logEvent.Object);
            logEvent.Type = (int)type;
            logEvent.Payload = source;
        }

    }

    public void Encode( LogEvent logEvent )
    {

        if( !string.IsNullOrWhiteSpace(logEvent.Payload) && string.IsNullOrWhiteSpace(logEvent.Base64) )
            logEvent.Base64 = WatchPayloadEncoder.Encode(logEvent.Payload);

    }


}



