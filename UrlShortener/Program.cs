using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UrlShortener.Data;
using UrlShortener.Middleware;

var builder = WebApplication.CreateBuilder(args);
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:Host"] ?? "localhost:6379");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.SwaggerDoc("v1", new() { Title = "URL Shortener API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database")
    .AddRedis(redis);

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

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowAll");
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
