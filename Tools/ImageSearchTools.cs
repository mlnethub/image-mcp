using Extensions;

using Microsoft.Extensions.Options;

using ModelContextProtocol.Server;

using Options;

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
            var normalizedCount = Math.Clamp(count, 1, 5);
            var normalizedOrientation = NormalizeOrientation(orientation);
            var trimmedQuery = query?.Trim();

            var urlBuilder = new StringBuilder($"photos/random?client_id={clientId}&count={normalizedCount}&orientation={normalizedOrientation}");
            if (!string.IsNullOrEmpty(trimmedQuery))
            {
                urlBuilder.Append("&query=");
                urlBuilder.Append(Uri.EscapeDataString(trimmedQuery));
            }

            var url = urlBuilder.ToString();

            using var jsonDocument = await client.ReadJsonDocumentAsync(url);
            var root = jsonDocument.RootElement;

            return ParseResults(root);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting random images: {ex.Message}");
            return Enumerable.Empty<ImageResult>();
        }

        static string NormalizeOrientation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "landscape";
            }

            var normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "landscape":
                case "portrait":
                case "squarish":
                    return normalized;
                default:
                    Console.Error.WriteLine($"Unsupported orientation '{value}', falling back to 'landscape'.");
                    return "landscape";
            }
        }

        static IEnumerable<ImageResult> ParseResults(JsonElement root)
        {
            return root.ValueKind switch
            {
                JsonValueKind.Array => MapArray(root),
                JsonValueKind.Object => MapObject(root),
                _ => Enumerable.Empty<ImageResult>()
            };
        }

        static IEnumerable<ImageResult> MapArray(JsonElement root)
        {
            var results = new List<ImageResult>();

            foreach (var element in root.EnumerateArray())
            {
                var mapped = MapElement(element);
                if (mapped != null)
                {
                    results.Add(mapped);
                }
            }

            return results;
        }

        static IEnumerable<ImageResult> MapObject(JsonElement element)
        {
            var mapped = MapElement(element);
            return mapped != null
                ? new[] { mapped }
                : Enumerable.Empty<ImageResult>();
        }

        static ImageResult? MapElement(JsonElement element)
        {
            try
            {
                var image = JsonSerializer.Deserialize<Image>(element.GetRawText());
                if (image == null)
                {
                    return null;
                }

                return new ImageResult
                {
                    Urls = image.Urls ?? new ImageUrls(),
                    Description = image.Description,
                    AltDescription = image.AltDescription
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}