using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Options;

using Tools;

namespace ImageMcp.Tests.Tools;

[TestClass]
public class ImageSearchToolsErrorTests
{
    [TestMethod]
    public async Task GetRandomImages_WhenHttpRequestFails_ReturnsEmpty()
    {
        var handler = new FailingHttpMessageHandler(new HttpRequestException("boom"));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/")
        };
        var options = CreateOptions();

        var results = await ImageSearchTools.GetRandomImages(client, options);

        Assert.IsFalse(results.Any());
    }

    [TestMethod]
    public async Task GetRandomImages_WhenJsonIsInvalid_ReturnsEmpty()
    {
        var handler = new RawPayloadHttpMessageHandler("not-json");
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/")
        };
        var options = CreateOptions();

        var results = await ImageSearchTools.GetRandomImages(client, options);

        Assert.IsFalse(results.Any());
    }

    private static IOptions<ImageApiOptions> CreateOptions()
    {
        return Microsoft.Extensions.Options.Options.Create(new ImageApiOptions
        {
            BaseUrl = "https://api.unsplash.com/",
            ClientId = "test-client"
        });
    }

    private sealed class FailingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public FailingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(_exception);
        }
    }

    private sealed class RawPayloadHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public RawPayloadHttpMessageHandler(string payload)
        {
            _response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
