using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;
using System.Threading.RateLimiting;
using UrlShortener.Data;
using UrlShortener.Middleware;

var builder = WebApplication.CreateBuilder(args);
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:Host"] ?? "localhost:6379");

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.SwaggerDoc("v1", new() { Title = "URL Shortener API", Version = "v1" });
});


//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

//Rate Limiter Service
builder.Services.AddRateLimiter(option =>
{
    //option.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    option.OnRejected = async (context, token) =>
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"Rate limit triggered for IP {ip} at {DateTime.UtcNow}");

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded. Please wait before retrying."
        }, cancellationToken: token);
    };

    option.AddPolicy("PerIpLimit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});

//Health Check Service
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database")
    .AddRedis(redis);

//Correlation Id
builder.Services.AddDefaultCorrelationId(options =>
{
    options.AddToLoggingScope = true;
    options.UpdateTraceIdentifier = true;
    options.IncludeInResponse = true;
});

//Logger
builder.Host.UseSerilog((context, config) =>
{
    config
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(context.Configuration);
});

builder.WebHost.UseUrls("http://+:80");

var app = builder.Build();

app.MapHealthChecks("/health");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "UrlShorterner API");
    });
}

app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");
app.UseRateLimiter();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
