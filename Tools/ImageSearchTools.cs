using Extensions;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

using Options;

using System.ComponentModel;
using System.Text.Json;

using image_mcp.Models;

namespace Tools;


[McpServerToolType]
public static class ImageSearchTools
{
    [McpServerTool, Description("Search for images based on a query string and returns an array of image results.")]
    public static async Task<IEnumerable<ImageResult>> SearchImages(
        HttpClient client,
        IOptions<ImageApiOptions> options,
        [Description("The query string to search for images.")] string query)
    {
        try
        {
            var clientId = options.Value.ClientId;

            const int perPage = 10;
            const string contentFilter = "high";
            const string orderBy = "relevant";

            using var jsonDocument = await client.ReadJsonDocumentAsync($"search/photos?client_id={clientId}&per_page={perPage}&content_filter={contentFilter}&order_by={orderBy}&query={Uri.EscapeDataString(query)}");
            var jsonElement = jsonDocument.RootElement;
            var results = jsonElement.GetProperty("results").EnumerateArray();

            if (!results.Any())
            {
                return Enumerable.Empty<ImageResult>();
            }

            var images = results
                .Select(result => JsonSerializer.Deserialize<Image>(result.GetRawText()))
                .Where(img => img != null)
                .Select(img => new ImageResult
                {
                    Urls = img!.Urls,
                    Description = img.Description,
                    AltDescription = img.AltDescription
                })
                .ToList();

            return images;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error searching images: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

            return new List<ImageResult>
            {
                new ImageResult
                {
                    Error = $"Error: {ex.Message}",
                    Urls = new ImageUrls()
                }
            };
        }
    }

    [McpServerTool, Description("Get random images for inspiration or background based on optional query.")]
    public static async Task<IEnumerable<ImageResult>> GetRandomImages(
        HttpClient client,
        IOptions<ImageApiOptions> options,
        [Description("Optional query to filter images (e.g. 'nature', 'cats').")] string? query = null,
        [Description("Number of images to return. Default is 1.")] int count = 1,
        [Description("Image orientation: 'landscape' (default), 'portrait', or 'squarish'.")] string orientation = "landscape")
    {
        try
        {
            var clientId = options.Value.ClientId;

            var url = $"photos/random?client_id={clientId}&count={count}&orientation={orientation}";
            if (!string.IsNullOrWhiteSpace(query))
            {
                url += $"&query={Uri.EscapeDataString(query)}";
            }

            using var jsonDocument = await client.ReadJsonDocumentAsync(url);
            var jsonElement = jsonDocument.RootElement;

            // When 'count' is provided, API returns an array.
            // If API changes or somehow returns an object, we handle it loosely.
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var results = jsonElement.EnumerateArray();
                var images = results
                    .Select(result => JsonSerializer.Deserialize<Image>(result.GetRawText()))
                    .Where(img => img != null)
                    .Select(img => new ImageResult
                    {
                        Urls = img!.Urls,
                        Description = img.Description,
                        AltDescription = img.AltDescription
                    })
                    .ToList();
                return images;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                 var img = JsonSerializer.Deserialize<Image>(jsonElement.GetRawText());
                 if (img != null)
                 {
                    return new List<ImageResult>
                    {
                        new ImageResult
                        {
                            Urls = img.Urls,
                            Description = img.Description,
                            AltDescription = img.AltDescription
                        }
                    };
                 }
            }

            return Enumerable.Empty<ImageResult>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting random images: {ex.Message}");
            return new List<ImageResult>
            {
                new ImageResult
                {
                    Error = $"Error: {ex.Message}",
                    Urls = new ImageUrls()
                }
            };
        }
    }
}