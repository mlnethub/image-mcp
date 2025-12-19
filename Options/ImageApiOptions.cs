using System.ComponentModel.DataAnnotations;

namespace Options;

public class ImageApiOptions
{
  [Required]
  public string BaseUrl { get; set; } = string.Empty;

  [Required]
  public string ClientId { get; set; } = string.Empty;
}
