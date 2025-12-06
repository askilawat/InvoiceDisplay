using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using BuggyApp.Data;
using BuggyApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Invoice API",
        Version = "v1",
        Description = "API for managing invoices"
    });
});

builder.Services.AddDbContext<InvoiceDbContext>(options =>
    options.UseSqlite("Data Source=invoices.db"));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure Swagger
// Enable Swagger in all environments (can be disabled via ENABLE_SWAGGER=false)
var enableSwagger = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
if (enableSwagger != "false")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice API V1");
    });
}

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapFallbackToFile("index.html");

// Configure to listen on all interfaces for cloud deployment
// Render sets PORT environment variable (typically 10000)
// ASP.NET Core automatically reads ASPNETCORE_URLS if set
// If PORT is set but ASPNETCORE_URLS is not, configure it manually
var port = Environment.GetEnvironmentVariable("PORT");
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrEmpty(port) && string.IsNullOrEmpty(urls))
{
    // Render and platforms that only set PORT
    app.Urls.Add($"http://+:{port}");
}

app.Run();

