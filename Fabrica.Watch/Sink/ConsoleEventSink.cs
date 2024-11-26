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

namespace Fabrica.Watch.Sink;

public class ConsoleEventSink: IEventSinkProvider
{


    public virtual void Start()
    {

    }

    public virtual void Stop()
    {

    }


    public virtual Task Accept( LogEventBatch batch )
    {

        foreach ( var le in batch.Events )
            _write(le);

        return Task.CompletedTask;

    }


    private void _write( LogEvent le)
    {

        switch (le.Level)
        {
            case (int)Level.Trace:
            case (int)Level.Debug:
                Console.ForegroundColor = ConsoleColor.Green;
                break;

            case (int)Level.Info:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;

            case (int)Level.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case (int)Level.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case (int)Level.Quiet:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }


        Console.WriteLine("================================================================================");

        var message = $"{le.Occurred:T} - {le.Level.ToString().ToUpper()} - {le.Category} - {le.Title}";
        Console.Out.WriteLine(message);
        if (le.Type != (int)PayloadType.None)
        {
            Console.Out.WriteLine("--------------------------------------------------------------------------------");
            Console.Out.WriteLine(le.Payload);
        }
        Console.ResetColor();

    }


}