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

using System.Text.Json;
using MemoryPack;
using MemoryPack.Compression;
using Microsoft.IO;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Fabrica.Watch.Utilities;

namespace Fabrica.Watch.Sink;

[MemoryPackable]
public partial class LogEvent: IDisposable
{

    public static LogEvent Single { get; } = new ();


    [MemoryPackIgnore]
    internal Action<LogEvent> OnDispose { get; set; } = _ => { };

    public void Dispose()
    {
        OnDispose(this);
    }

    public void Reset()
    {
        Category = string.Empty;
        CorrelationId = string.Empty;
        Title = string.Empty;
        Tenant = string.Empty;
        Subject = string.Empty;
        Tag = string.Empty;
        Level = 0;
        Color = 0;
        Nesting = 0;
        Occurred = WatchHelpers.ToWatchTimestamp();
        Type = 0;
        Object = null;
        Error = null;
        ErrorContext = null;
        Payload = null;
        Base64 = null;
    }


    public string Category { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;

    public int Level { get; set; }
    public int Color { get; set; }
    public int Nesting { get; set; }

    public long Occurred { get; set; }

    public int Type { get; set; }
    public string? Base64 { get; set; }


    [JsonIgnore]
    [MemoryPackIgnore]
    public object? Object { get; set; }


    [JsonIgnore]
    [MemoryPackIgnore]
    public Exception? Error { get; set; }
    [JsonIgnore]
    [MemoryPackIgnore]
    public object? ErrorContext { get; set; }


    [JsonIgnore]
    [MemoryPackIgnore]
    public string? Payload { get; set; }


}

[MemoryPackable]
public partial class LogEventBatch
{

    public static readonly LogEventBatch Empty = new ();

    public static LogEventBatch Single( string domain, LogEvent one )
    {
        return new LogEventBatch { Domain = domain, Events = [one] };
    }

    public string Uid { get; private set; } = Ulid.NewUlid();
    public string Domain { get; set; } = string.Empty;

    public List<LogEvent> Events { get; set; } = [];

}

public static class LogEventBatchSerializer
{

    private static readonly RecyclableMemoryStreamManager Manager = new ();

    public static async Task<Stream> ToStream( LogEventBatch batch )
    {

        using var compressor = new BrotliCompressor();

        MemoryPackSerializer.Serialize(compressor, batch);

        var stream = Manager.GetStream();

        await compressor.CopyToAsync(stream);

        return stream;

    }


    public static async Task ToStream( LogEventBatch batch, Stream target )
    {

        using var compressor = new BrotliCompressor();

        MemoryPackSerializer.Serialize(compressor, batch);

        await compressor.CopyToAsync(target);

    }

    public static async Task<LogEventBatch?> FromStream( Stream source )
    {

        await using var stream = Manager.GetStream();
        await source.CopyToAsync(stream);
        stream.Position = 0;

        using var decompressor = new BrotliDecompressor();

        var ros = decompressor.Decompress( stream.GetReadOnlySequence() );

        var batch = MemoryPackSerializer.Deserialize<LogEventBatch>(ros);

        return batch;

    }


    public static string ToJson( LogEventBatch batch )
    {

        var json = JsonSerializer.Serialize(batch, LogEventBatchContext.Default.LogEventBatch);

        return json;

    }


    public static LogEventBatch? FromJson( string json )
    {

        var batch = JsonSerializer.Deserialize( json, LogEventBatchContext.Default.LogEventBatch );

        return batch;

    }

}




[JsonSourceGenerationOptions(JsonSerializerDefaults.General)]
[JsonSerializable(typeof(LogEvent))]
[JsonSerializable(typeof(LogEventBatch))]
public partial class LogEventBatchContext : JsonSerializerContext;


