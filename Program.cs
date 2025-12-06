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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice API V1");
});

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

