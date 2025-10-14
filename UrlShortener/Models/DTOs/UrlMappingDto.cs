using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models.DTOs
{
    public class UrlMappingDto
    {
        [Required]
        public required string OriginalUrl { get; set; }
    }
}
