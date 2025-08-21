/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

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
using System.Collections.Concurrent;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Local

namespace Fabrica.Watch;

public class WatchFactoryConfig
{

    public bool Quiet { get; init; }

    public int InitialPoolSize { get; set; } = 50;
    public int MaxPoolSize { get; set; } = 500;

    public int BatchSize { get; set; } = 10;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
    public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);

    public required ISwitchSource Switches { get; init; }

    public required IEnumerable<IEventSinkProvider> Sinks { get; init; }


}

public abstract class AbstractWatchFactory( WatchFactoryConfig config ) : IWatchFactory
{

    private static readonly ILogger Silencer = new QuietLogger();

    private IWatchObjectSerializer ForObject { get; set; } = new JsonWatchObjectSerializer();
    private IWatchExceptionSerializer ForException { get; set; } = new TextExceptionSerializer();


    private int InitialPoolSize { get;  } = config.InitialPoolSize;
    private int MaxPoolSize { get; } = config.MaxPoolSize;
    private Pool<Logger> LoggerPool { get; set; } = null!;
    private Pool<LogEvent> EventPool { get; set; } = null!;

    protected List<IEventSinkProvider> Sinks { get; } = [..config.Sinks];

    private bool Quiet { get; } = config.Quiet;

    public ISwitchSource Switches { get; private set; } = config.Switches;

    private void _return(Logger lg)
    {
         LoggerPool.Return(lg);
    }

    private void _return(LogEvent le)
    {
        EventPool.Return(le);
    }

    protected bool Started { get; set; }
    public virtual async Task Start()
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


        Switches.Start();

        foreach (var sink in Sinks)
            await sink.Start();

        Started = true;

    }


    public virtual async Task Stop()
    {

        try
        {
            Switches.Stop();
            Switches = null!;
        }
        catch
        {
            //ignored
        }

        try
        {

            foreach (var sink in Sinks)
                await sink.Stop();

            Sinks.Clear();

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


    public abstract void Accept(LogEvent logEvent);

    
    public virtual ILogger GetLogger( string category, bool retroOn=true )
    {

        // Overall Quiet
        if( Quiet )
            return Silencer;


        // Get Switch for given Category
        var sw = Switches.Lookup( category );


        // Return the silencer if switch level is quiet
        if (sw.Level is Level.Quiet)
            return Silencer;


        // Acquire a new logger from the pool
        var logger = LoggerPool.Acquire(0);



        // Config the acquired logger
        logger.Config( this, string.Empty, string.Empty, sw.Tag, category, string.Empty, sw.Level, sw.Color );


        // **************************************
        return logger;

    }


    public virtual ILogger GetLogger<T>( bool retroOn = true )
    {

        var logger = GetLogger( typeof(T).FullName ?? string.Empty, retroOn );

        return logger;

    }



    public virtual ILogger GetLogger( Type type, bool retroOn = true)
    {

        var logger = GetLogger(type.GetConciseFullName() ?? string.Empty, retroOn );

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



        // Return the silencer if switch level is quiet
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


public class BackgroundWatchFactory(WatchFactoryConfig config): AbstractWatchFactory(config)
{
    
    private int BatchSize { get;  } = config.BatchSize;
    private TimeSpan PollingInterval { get;  } = config.PollingInterval;
    private TimeSpan WaitForStopInterval { get;  } = config.WaitForStopInterval;
    
    private ConcurrentQueue<LogEvent> Queue { get; } = new();
    private CancellationTokenSource MustStop { get; } = new ();
    
    public override void Accept( LogEvent logEvent )
    {

        Enrich(logEvent);
        Encode(logEvent);

        Queue.Enqueue(logEvent);

    }

    public override async Task Start()
    {
        
        await base.Start();
        
#pragma warning disable CS4014
        Task.Run(_process);
#pragma warning restore CS4014
       
        
    }

    
    public override async Task Stop()
    {

        if (!Started)
            return;

        Started = false;

        await MustStop.CancelAsync();        
        
        await base.Stop();
        
    }


    private async Task _process()
    {

        while (!MustStop.IsCancellationRequested)
        {
            await Drain(false);
        }

        await Drain(true);

    }

    private readonly LogEventBatch _batch = new();

    private async Task Drain(bool all)
    {

        if( Queue.IsEmpty )
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            return;
        }

        _batch.Events.Clear();

        while (!Queue.IsEmpty)
        {

            if (!all && _batch.Events.Count >= BatchSize)
                break;

            if (!Queue.TryDequeue(out var le))
                break;

            _batch.Events.Add(le);

        }

        if (_batch.Events.Count > 0)
        {

            foreach (var sink in Sinks)
                await sink.Accept(_batch);

            _batch.Events.ForEach(e => e.Dispose());

        }


    }    
    
    
}


public class ForegroundWatchFactory(WatchFactoryConfig config) : AbstractWatchFactory(config)
{
    
    public override void Accept( LogEvent logEvent )
    {

        var batch = LogEventBatch.Single( "",  logEvent );

        foreach (var sink in Sinks)
        {
            sink.Accept(batch).SafeFireAndForget(_handleException);
        }
        
    }

    private void _handleException(Exception cause)
    {
        
    }    
    
    
}