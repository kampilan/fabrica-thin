/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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

using Fabrica.Watch.Sink;
using Gurock.SmartInspect;
using Microsoft.IO;
using System.Text;

namespace Fabrica.Watch.Realtime;

public class RealtimeSink: IEventSinkProvider
{

    private static readonly RecyclableMemoryStreamManager Manager = new();

    private SmartInspect Si { get; set; } = null!;

    public Task Start()
    {

        Si = new SmartInspect("Fabrica")
        {
            Connections = "tcp(host=localhost, reconnect=true, reconnect.interval=10)",
            Enabled = true,
            Level = Gurock.SmartInspect.Level.Debug
        };

        return Task.CompletedTask;

    }

    public Task Stop()
    {
        Si.Enabled = false;
        Si.Dispose();

        return Task.CompletedTask;

    }

    public Task Accept( LogEventBatch batch, CancellationToken ct=default )
    {

        foreach( var le in batch.Events )
        {
            var entry = _mapToLogEntry(le);
            Si.SendLogEntry(entry);
        }

        return Task.CompletedTask;

    }


    private static Gurock.SmartInspect.Level _mapToSILevel(int level)
    {
        return level switch
        {
            (int)Level.Trace => Gurock.SmartInspect.Level.Debug,
            (int)Level.Debug => Gurock.SmartInspect.Level.Debug,
            (int)Level.Info => Gurock.SmartInspect.Level.Message,
            (int)Level.Warning => Gurock.SmartInspect.Level.Warning,
            (int)Level.Error => Gurock.SmartInspect.Level.Error,
            _ => Gurock.SmartInspect.Level.Fatal
        };
    }


    private static LogEntry _mapToLogEntry( LogEvent le )
    {


        var entryType = LogEntryType.Debug;
        switch (le.Level)
        {
            case (int)Level.Trace:
                entryType = LogEntryType.Debug;
                break;

            case (int)Level.Debug:
                entryType = LogEntryType.Debug;
                break;

            case (int)Level.Info:
                entryType = LogEntryType.Message;
                break;

            case (int)Level.Warning:
                entryType = LogEntryType.Warning;
                break;

            case (int)Level.Quiet:
            case (int)Level.Error:
                entryType = LogEntryType.Error;
                break;

        }


        if (le.Nesting == 1)
            entryType = LogEntryType.EnterMethod;
        else if (le.Nesting == -1)
            entryType = LogEntryType.LeaveMethod;


        MemoryStream data = null!;
        var viewerId  = ViewerId.Title;
        if( le.Type != (int)PayloadType.None && !string.IsNullOrWhiteSpace(le.Base64) )
        {

            var buf = Convert.FromBase64String(le.Base64);
            var clear = Encoding.ASCII.GetString(buf);

            data = Manager.GetStream();
            var writer = new StreamWriter(data);
            writer.Write(clear);
            writer.Flush();
            data.Seek(0, SeekOrigin.Begin);

            switch (le.Type)
            {

                case (int)PayloadType.Json:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.JavaScriptSource;
                    break;

                case (int)PayloadType.Sql:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.SqlSource;
                    break;

                case (int)PayloadType.Xml:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.XmlSource;
                    break;

                case (int)PayloadType.Text:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.Data;
                    break;

                case (int)PayloadType.Yaml:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.JavaScriptSource;
                    break;

            }

        }

        var entry = new LogEntry
        {
            AppName       = "Fabrica",
            Level         = _mapToSILevel(le.Level),
            SessionName   = le.Category,
            Title         = le.Title,
            LogEntryType  = entryType,
            ViewerId      = viewerId,
            Timestamp     = le.Occurred,
            Data          = data
        };


        var rgb = le.Color & 0xffffff;
        entry.Color = System.Drawing.Color.FromArgb(rgb);

        return entry;

    }



}