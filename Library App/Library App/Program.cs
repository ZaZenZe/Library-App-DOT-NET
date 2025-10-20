using Library_App.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database context - prioritize environment variables over appsettings.json
var connectionString = Environment.GetEnvironmentVariable("Library_AppContextConnection") 
    ?? builder.Configuration.GetConnectionString("Library_AppContextConnection");

builder.Services.AddDbContext<Library_AppContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
