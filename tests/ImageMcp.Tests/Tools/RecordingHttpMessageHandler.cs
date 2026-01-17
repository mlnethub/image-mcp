using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageMcp.Tests.Tools;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public RecordingHttpMessageHandler()
    {
        _response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        };
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_response);
    }

    public HttpClient CreateClient()
    {
        return new HttpClient(this)
        {
            BaseAddress = new Uri("https://example.com/")
        };
    }

    public string? GetQueryParameter(string name)
    {
        if (LastRequest?.RequestUri == null)
        {
            return null;
        }

        var query = LastRequest.RequestUri.Query;
        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        var trimmed = query.TrimStart('?');
        var segments = trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            var parts = segment.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], name, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
