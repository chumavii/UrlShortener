using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using StackExchange.Redis;
using System.Net.Http.Json;
using UrlShortener;
using UrlShortener.Controllers;
using UrlShortener.Data;
using UrlShortener.Models;
using UrlShortener.Models.DTOs;

namespace UrlShortener.Tests
{
    public class UrlControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly UrlController _controller;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConnectionMultiplexer> _redisMock;

        public UrlControllerTests(WebApplicationFactory<Program> factory)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            var mockDatabase = new Mock<IDatabase>();
            mockDatabase
                .Setup(db => db.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()
                ))
                .ReturnsAsync(true);

            mockDatabase
                .Setup(db => db.StringGetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<CommandFlags>()
                ))
                .ReturnsAsync((RedisValue)string.Empty);
            _redisMock = new Mock<IConnectionMultiplexer>();
            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                      .Returns(mockDatabase.Object);
            _controller = new UrlController(_context, _redisMock.Object);
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task HealthEndpoint_ShouldReturnSuccess()
        {
            //Act
            var response = await _client.GetAsync("/health");

            //Assert
            Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.OK ||
            response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable,
            $"Unexpected status: {response.StatusCode}"
            );
        }

        [Fact]
        public async Task ShortenUrl_EmptyUrl_ShouldReturnBadRequest()
        {
            //Arrange
            var urlDto = new UrlMappingDto { OriginalUrl = "" };

            //Act
            var result = await _controller.ShortenUrl(urlDto);

            //Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("URL cannot be empty", badRequest.Value);
        }

        [Fact]
        public async Task ShortenUrl_AddsHttpsIfMissinge()
        {
            //Arrange
            var dto = new UrlMappingDto { OriginalUrl = "www.testr.com" };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Request =
                    {
                        Scheme = "https",
                        Host = new HostString("localhost:8080")
                    }
                }
            };

            //Act
            var result = await _controller.ShortenUrl(dto) as OkObjectResult;
            var response = result?.Value;
            var shortUrl = response?.GetType().GetProperty("shortUrl")?.GetValue(response, null)?.ToString();

            //Assert
            Assert.StartsWith("https", shortUrl);

        }

        [Theory]
        [InlineData("User-Agent", "postman")]
        [InlineData("Sec-Fetch-Mode", "cors")]
        [InlineData("Referer", "swagger")]
        public async Task IsBrowserRequest_IfNotBrowser_Returns200Ok(string headerName, string headerValue)
        {
            //Arrange
            var record = new UrlMapping
            {
                OriginalUrl = "www.test.com",
                ShortCode = "cEdfxXXd",
                CreatedAt = DateTime.UtcNow
            };
            _context.UrlMappings.Add(record);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers[headerName] = headerValue;

            //Act
            var result = await _controller.RedirectToOriginal("cEdfxXXd");

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task IsBrowserRequest_IfEmpty_ReturnsNullException()
        {
            //Arrange
            var record = new UrlMapping
            {
                OriginalUrl = "www.test.com",
                ShortCode = "cEdfxXXd",
                CreatedAt = DateTime.UtcNow
            };
            _context.UrlMappings.Add(record);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = null!
            };

            //Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.RedirectToOriginal("cEdfxXXd"));

            //Assert
            Assert.Contains("HTTP request cannot be null", ex.Message);
        }

    }
}