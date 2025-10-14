namespace UrlShortener.Models
{
    public class UrlMapping
    {
        public int id { get; set; }
        public required string OriginalUrl { get; set; }
        public string? ShortCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
