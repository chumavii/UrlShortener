using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(IConnectionMultiplexer redis) : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis = redis;

        [HttpGet("redit-test")]
        public async Task<IActionResult> RedisTest()
        {
            var db = _redis.GetDatabase();

            await db.StringSetAsync("test", "hello");

            var value = (await db.StringGetAsync("test")).ToString();

            return Ok(new { value });
        }
    }
}
