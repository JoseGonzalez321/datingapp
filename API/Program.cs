using API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors(policy =>
{
    policy.AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins(
              "https://localhost:5001",
              "chrome-extension://amknoiejhlmhancpahfcfcfhllgkpbld"
          );
});

app.MapControllers();

app.Run();
