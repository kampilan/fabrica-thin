
using System.Collections.Concurrent;

namespace Fabrica.Watch.Sink;

public class CompositeSink: IEventSink
{


    private ConcurrentQueue<LogEvent> Queue { get; } = new();
    private EventWaitHandle MustStop { get; } = new(false, EventResetMode.ManualReset);
    private EventWaitHandle Stopped { get; } = new(false, EventResetMode.ManualReset);


    public int BatchSize { get; set; } = 10;
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
    public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);



    private IList<IEventSinkProvider> Sinks { get; } = new List<IEventSinkProvider>();

    public IEnumerable<IEventSinkProvider> InnerSinks => Sinks;


    public void AddSink(IEventSinkProvider sink)
    {

        if( !_started )
            Sinks.Add( sink );

    }

    private bool _started;
    public void Start()
    {

        if (_started)
            return;

        foreach( var sink in Sinks )
            sink.Start();

        Task.Run(_process);

        _started = true;

    }

    public void Stop()
    {

        MustStop.Set();

        Stopped.WaitOne(WaitForStopInterval);

        foreach (var sink in Sinks)
            sink.Stop();

        Sinks.Clear();

    }


    public void Accept( LogEvent logEvent )
    {

        WatchFactoryLocator.Factory.Encode(logEvent);

        Queue.Enqueue( logEvent );

    }


    private async Task _process()
    {

        while (!MustStop.WaitOne(PollingInterval))
            await Drain(false);

        await Drain(true);

        Stopped.Set();

    }

    private readonly LogEventBatch _batch = new();

    protected virtual async Task Drain(bool all)
    {

        if (Queue.IsEmpty)
            return;

        _batch.Events.Clear();

        while (!Queue.IsEmpty)
        {

            if (!all && _batch.Events.Count >= BatchSize)
                break;

            if (!Queue.TryDequeue(out var le))
                break;

            _batch.Events.Add(le);

        }

        if( _batch.Events.Count > 0 )
        {

            foreach( var sink in Sinks)
                await sink.Accept(_batch);

            _batch.Events.ForEach(e=>e.Dispose());

        }
            

    }


}