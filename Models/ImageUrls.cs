using System.Text.Json.Serialization;

namespace image_mcp.Models;

public class ImageUrls
{
  [JsonPropertyName("raw")]
  public string Raw { get; set; } = string.Empty;

  [JsonPropertyName("full")]
  public string Full { get; set; } = string.Empty;

  [JsonPropertyName("regular")]
  public string Regular { get; set; } = string.Empty;

  [JsonPropertyName("small")]
  public string Small { get; set; } = string.Empty;

  [JsonPropertyName("thumb")]
  public string Thumb { get; set; } = string.Empty;
}
