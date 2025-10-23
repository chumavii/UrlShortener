using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using StackExchange.Redis;

namespace UrlShortener.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occured");
                await HandleExceptionAsync(context, ex)
;            }
        }

        public static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception switch
            {
                DbUpdateException => (int)HttpStatusCode.InternalServerError,
                RedisConnectionException => (int)HttpStatusCode.ServiceUnavailable,
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var response = new
            {
                message = exception.Message,
                stackTrace = exception.StackTrace
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
