using System.Net.Http.Headers;
using Fabrica.Watch.Sink;

namespace Fabrica.Watch.Http.Sink;

public class BinaryHttpEventSinkProvider : AbstractHttpEventSinkProvider
{

    private static readonly MediaTypeHeaderValue HeaderValue = MediaTypeHeaderValue.Parse("application/octet-stream");

    protected override async Task<HttpContent> BuildContentAsync(LogEventBatch batch)
    {

        var stream = await LogEventBatchSerializer.ToStream( batch );
        var content = new StreamContent(stream);
        content.Headers.ContentType = HeaderValue;

        return content;

    }

}