using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Options;

using Tools;

namespace ImageMcp.Tests.Tools;

[TestClass]
public class ImageSearchToolsParsingTests
{
    [TestMethod]
    public async Task GetRandomImages_WhenResponseIsArray_MapsToResults()
    {
        var images = new[]
        {
            new ImageStub("desc1", "alt1", "https://example.com/1"),
            new ImageStub("desc2", "alt2", "https://example.com/2")
        };

        var client = CreateClient(images);
        var options = CreateOptions();

        var results = (await ImageSearchTools.GetRandomImages(client, options)).ToList();

        Assert.AreEqual(2, results.Count);
        Assert.AreEqual("desc1", results[0].Description);
        Assert.AreEqual("alt2", results[1].AltDescription);
        Assert.AreEqual("https://example.com/2", results[1].Urls?.Regular);
    }

    [TestMethod]
    public async Task GetRandomImages_WhenResponseIsObject_WrapsSingleResult()
    {
        var image = new ImageStub("single", "alt", "https://example.com/s");

        var client = CreateClient(image);
        var options = CreateOptions();

        var results = (await ImageSearchTools.GetRandomImages(client, options)).ToList();

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("single", results[0].Description);
    }

    [TestMethod]
    public async Task GetRandomImages_WhenResponseIsEmpty_ReturnsEmptyList()
    {
        var client = CreateClient(Array.Empty<ImageStub>());
        var options = CreateOptions();

        var results = (await ImageSearchTools.GetRandomImages(client, options)).ToList();

        Assert.AreEqual(0, results.Count);
    }

    private static HttpClient CreateClient(params ImageStub[] images)
    {
        var payload = images.Length switch
        {
            0 => "[]",
            1 => JsonSerializer.Serialize(ToApiShape(images[0])),
            _ => JsonSerializer.Serialize(images.Select(ToApiShape))
        };

        var handler = new StaticHttpMessageHandler(payload);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/")
        };
    }

    private static IOptions<ImageApiOptions> CreateOptions()
    {
        return Microsoft.Extensions.Options.Options.Create(new ImageApiOptions
        {
            BaseUrl = "https://api.unsplash.com/",
            ClientId = "test-client"
        });
    }

    private sealed record ImageStub(string Description, string AltDescription, string Regular);

    private static object ToApiShape(ImageStub image)
    {
        return new
        {
            description = image.Description,
            alt_description = image.AltDescription,
            urls = new
            {
                regular = image.Regular,
                small = image.Regular,
                thumb = image.Regular
            }
        };
    }

    private sealed class StaticHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StaticHttpMessageHandler(string payload)
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
