using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Options;

using Tools;

namespace ImageMcp.Tests.Tools;

[TestClass]
public class ImageSearchToolsOrientationTests
{
    [TestMethod]
    public async Task GetRandomImages_WhenOrientationUppercase_UsesLowercase()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, orientation: "LANDSCAPE");

        Assert.AreEqual("landscape", handler.GetQueryParameter("orientation"));
    }

    [TestMethod]
    public async Task GetRandomImages_WhenOrientationMixedCasePortrait_UsesLowercase()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, orientation: "Portrait");

        Assert.AreEqual("portrait", handler.GetQueryParameter("orientation"));
    }

    [TestMethod]
    public async Task GetRandomImages_WhenOrientationInvalid_FallsBackToLandscape()
    {
        var handler = new RecordingHttpMessageHandler();
        var client = handler.CreateClient();
        var options = CreateOptions();

        await ImageSearchTools.GetRandomImages(client, options, orientation: "diagonal");

        Assert.AreEqual("landscape", handler.GetQueryParameter("orientation"));
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
