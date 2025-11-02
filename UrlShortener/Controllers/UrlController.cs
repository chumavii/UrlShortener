using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Models.DTOs;
using Utilities.Encode;

namespace UrlShortener.Controllers
{
    [EnableRateLimiting("PerIpLimit")]
    [ApiController]
    [Route("/")]
    public class UrlController(ApplicationDbContext context, IConnectionMultiplexer redis) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        public readonly IConnectionMultiplexer _redis = redis;

        /// <summary>
        /// Shortens the given URL and returns a short code.
        /// </summary>
        /// <returns>Short url but only short code is posted to db.</returns>
        [HttpPost]
        public async Task<ActionResult> ShortenUrl(UrlMappingDto model)
        {
            if (string.IsNullOrEmpty(model.OriginalUrl))
                return BadRequest("URL cannot be empty");

            var originalUrl = EnsureUrlHasScheme(model.OriginalUrl);

            //Url scheme needs to be valid
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                return BadRequest("Invalid URL format.");

            //If record already exists in cache return short url
            var db = _redis.GetDatabase();
            var cachedCode = await db.StringGetAsync($"url:{originalUrl}");
            if (!cachedCode.IsNullOrEmpty)
                return Ok(new { shortUrl = $"{Request.Scheme}://{Request.Host}/{cachedCode}" });

            //If record is in db but not cache, add to cache then return short url
            var existing = await _context.UrlMappings.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
            if (existing != null)
            {
                try
                {
                    await db.StringSetAsync($"url:{existing.OriginalUrl}", existing.ShortCode);
                    await db.StringSetAsync(existing.ShortCode, existing.OriginalUrl);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to cache URL mapping for {existing.OriginalUrl}", e.Message);
                    return Ok(new { shortUrl = $"{Request.Scheme}://{Request.Host}/{existing.ShortCode}" });
                }
            }

            //short code needs to be unique before adding to db
            string shortCode;
            bool codeExists;
            do
            {
                shortCode = Url64Helper.Encode(originalUrl + Guid.NewGuid());
                codeExists = _context.UrlMappings.Any(c => c.ShortCode == shortCode);
            }
            while (codeExists);

            var mapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.UrlMappings.Add(mapping);
            await _context.SaveChangesAsync();
            await db.StringSetAsync(shortCode, originalUrl);
            await db.StringSetAsync($"url:{originalUrl}", shortCode);

            return Ok(new { shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}" });
        }


        /// <summary>
        /// Redirects from short URL to original URL if browser or returns original URL if api call
        /// </summary>
        [HttpGet("{shortCode}")]
        public async Task<ActionResult> RedirectToOriginal(string shortCode)
        {
            var db = _redis.GetDatabase();

            //Check if shortcode is stored in cache and return original URL if it is
            var cachedUrl = await db.StringGetAsync(shortCode);
            if (!cachedUrl.IsNullOrEmpty)
            {
                try
                {
                    if (IsBrowserRequest(Request))
                    {
                        Console.WriteLine("Is browser request: " + true);
                        return Redirect(EnsureUrlHasScheme(cachedUrl.ToString()));
                    }

                    Console.WriteLine("Is browser request: " + false);
                    return Ok(new { originalUrl = cachedUrl.ToString() });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            //If record isnt in cache, check database 
            var entity = await _context.UrlMappings.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
            if (entity == null) return NotFound("Short URL not found.");

            //Store record in cache
            await db.StringSetAsync(shortCode, entity.OriginalUrl);

            if (IsBrowserRequest(Request))
                return Redirect(entity.OriginalUrl);

            return Ok(new { originalUrl = entity.OriginalUrl });
        }

        private string EnsureUrlHasScheme(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                return "https://" + url;
            return url;
        }

        private static bool IsBrowserRequest(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "HTTP request cannot be null");

            if (request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                if (userAgent.ToString().ToLower().Contains("postman"))
                    return false;
            }

            if (request.Headers.TryGetValue("Sec-Fetch-Mode", out var mode))
            {
                if (mode.ToString().ToLower() == "cors")
                    return false;
            }

            if (request.Headers.TryGetValue("Referer", out var referer))
            {
                if (referer.ToString().ToLower().Contains("swagger"))
                    return false;
            }
            return true;
        }
    }
}
