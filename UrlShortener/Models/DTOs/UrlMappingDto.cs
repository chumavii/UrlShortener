using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models.DTOs
{
    public class UrlMappingDto
    {
        [Required(ErrorMessage = "URL cannot be empty")]
        public required string OriginalUrl { get; set; }
    }
}
