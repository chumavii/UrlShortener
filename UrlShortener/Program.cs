using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Data;

var builder = WebApplication.CreateBuilder(args);
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:Host"] ?? "localhost:6379");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "UrlShorterner API");
    });
}
builder.WebHost.UseUrls("http://+:80");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
