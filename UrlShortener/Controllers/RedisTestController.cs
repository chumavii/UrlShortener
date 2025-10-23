using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisTestController(IConnectionMultiplexer redis) : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis = redis;

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedisCodeTest(string shortCode)
        {
            var db = _redis.GetDatabase();

            var value = (await db.StringGetAsync(shortCode)).ToString();

            return Ok(new { value });
        }

        [HttpGet("url")]
        public async Task<IActionResult> RedisUrlTest([FromQuery] string url)
        {
            var db = _redis.GetDatabase();

            var value = (await db.StringGetAsync($"url:{url}")).ToString();
            if (string.IsNullOrEmpty(value))
                return NotFound($"No value found for key {url}");

            return Ok(new { value });
        }
    }
}
