using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Models.DTOs;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpPost]
        public async Task<ActionResult> ShortenUrl(UrlMappingDto model)
        {
            var mapping = new UrlMapping
            {
                OriginalUrl = model.OriginalUrl,
                ShortCode = "test@test.com",
                CreatedAt = DateTime.UtcNow
            };
            _context.UrlMappings.Add(mapping);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
    }
}
