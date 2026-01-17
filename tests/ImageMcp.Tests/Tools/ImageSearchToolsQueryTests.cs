using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Options;

using Tools;

namespace ImageMcp.Tests.Tools;

[TestClass]
public class ImageSearchToolsQueryTests
{
    [TestMethod]
    public async Task GetRandomImages_WhenQueryHasWhitespace_TrimsAndEncodes()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, query: "  space cat  ");

        var queryString = handler.LastRequest!.RequestUri!.Query;
        StringAssert.Contains(queryString, "query=space%20cat");
    }

    [TestMethod]
    public async Task GetRandomImages_WhenQueryHasSpecialCharacters_Encodes()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, query: "c++ sparks");

        var queryString = handler.LastRequest!.RequestUri!.Query;
        StringAssert.Contains(queryString, "query=c%2B%2B%20sparks");
    }

    [TestMethod]
    public async Task GetRandomImages_WhenQueryIsWhitespace_DoesNotSendParameter()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, query: "    ");

        var queryString = handler.LastRequest!.RequestUri!.Query;
        Assert.IsFalse(queryString.Contains("query=", StringComparison.OrdinalIgnoreCase));
    }

    private static IOptions<ImageApiOptions> CreateOptions()
    {
        return Microsoft.Extensions.Options.Options.Create(new ImageApiOptions
        {
            BaseUrl = "https://api.unsplash.com/",
            ClientId = "test-client"
        });
    }
}
