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

namespace Fabrica.Watch;

public class WatchFactory(int initialPoolSize = 50, int maxPoolSize = 500) : IWatchFactory
{

    private static readonly ILogger Silencer = new QuietLogger();

    private IWatchObjectSerializer ForObject { get; set; } = new JsonWatchObjectSerializer();
    private IWatchExceptionSerializer ForException { get; set; } = new TextExceptionSerializer();


    private int InitialPoolSize { get; set; } = initialPoolSize;
    private int MaxPoolSize { get; set; } = maxPoolSize;
    private Pool<Logger> LoggerPool { get; set; } = null!;
    private Pool<LogEvent> EventPool { get; set; } = null!;


    public bool Quiet { get; set; }

    public ISwitchSource Switches { get; set; } = null!;

    private CompositeSink _composite = null!;

    public IEventSink Sink
    {
        get => _composite;
        private set
        {
            if( value is CompositeSink cs )
                _composite = cs;
        }
    }

    public IEventSinkProvider? GetSink<T>() where T : class, IEventSinkProvider
    {

        var snk = _composite.InnerSinks.FirstOrDefault(s => s is T);
        return snk;

    }

    public virtual void Configure( ISwitchSource switches, IEventSink sink, bool quiet=false )
    {

        Quiet = quiet;

        Switches = switches ?? throw new ArgumentNullException(nameof(switches));
        Sink     = sink ?? throw new ArgumentNullException(nameof(sink));

        if( Quiet )
        {
            InitialPoolSize = 0;
            MaxPoolSize = 0;
        }

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


    }

    private void _return(Logger lg)
    {
         LoggerPool.Return(lg);
    }

    private void _return(LogEvent le)
    {
        EventPool.Return(le);
    }


    public virtual void Start()
    {

        Console.WriteLine("Factory Stopping");

        Switches.Start();

        Sink.Start();

    }


    public virtual void Stop()
    {

        Console.WriteLine("Factory Stopping");

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
            Sink.Stop();
            Sink = null!;
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


    }

    public int PooledCount => LoggerPool.Count;


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
        logger.Config( Sink, string.Empty, string.Empty, sw.Tag, category, string.Empty, sw.Level, sw.Color );


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

        var logger = GetLogger(type.FullName ?? string.Empty, retroOn );

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
        logger.Config( Sink, request.Tenant, request.Subject, sw.Tag, request.Category, request.CorrelationId, sw.Level, sw.Color );


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