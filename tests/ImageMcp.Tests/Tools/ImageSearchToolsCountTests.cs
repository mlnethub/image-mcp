using System;
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
public class ImageSearchToolsCountTests
{
    [TestMethod]
    public async Task GetRandomImages_WhenCountIsBelowMinimum_UsesOne()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, count: 0);

        Assert.AreEqual("1", handler.GetQueryParameter("count"));
    }

    [TestMethod]
    public async Task GetRandomImages_WhenCountExceedsMaximum_UsesFive()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, count: 10);

        Assert.AreEqual("5", handler.GetQueryParameter("count"));
    }

    [TestMethod]
    public async Task GetRandomImages_WhenCountNotProvided_UsesDefaultOne()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options);

        Assert.AreEqual("1", handler.GetQueryParameter("count"));
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
