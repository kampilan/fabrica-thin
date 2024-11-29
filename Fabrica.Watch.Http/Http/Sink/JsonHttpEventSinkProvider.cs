using System.Net.Http.Headers;
using System.Text;
using Fabrica.Watch.Sink;

namespace Fabrica.Watch.Http.Sink;

public class JsonHttpEventSinkProvider : AbstractHttpEventSinkProvider
{

    private static readonly MediaTypeHeaderValue HeaderValue = MediaTypeHeaderValue.Parse("application/json");

    protected override Task<HttpContent> BuildContentAsync(LogEventBatch batch)
    {

        var json = LogEventBatchSerializer.ToJson(batch);
        var content = new StringContent(json, Encoding.UTF8, HeaderValue );

        return Task.FromResult((HttpContent)content);

    }

}