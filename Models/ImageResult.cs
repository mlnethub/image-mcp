namespace image_mcp.Models;

public class ImageResult
{
  public ImageUrls Urls { get; set; } = new();
  public string? Description { get; set; }
  public string? AltDescription { get; set; }
  public string? Error { get; set; }
}
