using System.Collections.Concurrent;

namespace Fabrica.Watch.Sink;

public class MonitorSink: IEventSinkProvider
{

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    private ConcurrentQueue<LogEvent> Events { get; } = new();

    public bool Accumulate { get; set; }
    public TimeSpan WorkDelay { get; set; } = TimeSpan.MinValue;


    public int Total => _total;
    public int Count => Events.Count; 
    public IEnumerable<LogEvent> GetEvents() => Events;


    public void Flush()
    {
        Events.Clear();
    }


    private int _total;

    public async Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {

        foreach (var le in batch.Events)
        {

            if( Accumulate )
                Events.Enqueue(le);

            Interlocked.Increment(ref _total);

        }

        if(WorkDelay != TimeSpan.MinValue)
            await Task.Delay(WorkDelay);

    }

}