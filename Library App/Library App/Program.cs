using Library_App.Data;
using Library_App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database context - prioritize environment variables over appsettings.json
var connectionString = Environment.GetEnvironmentVariable("Library_AppContextConnection") 
    ?? builder.Configuration.GetConnectionString("Library_AppContextConnection");

builder.Services.AddDbContext<Library_AppContext>(options =>
    options.UseSqlServer(connectionString));

// Register services with dependency injection
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Run migrations and seed data
var skipMigrations = Environment.GetEnvironmentVariable("SKIP_DB_MIGRATIONS");
if (string.IsNullOrEmpty(skipMigrations) || skipMigrations.ToLower() != "true")
{
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<Library_AppContext>();
        
        // Apply pending migrations
        await context.Database.MigrateAsync();
        
        // Seed initial data
        await DbInitializer.SeedAsync(context);
    }
    catch (SqlException ex) when (ex.Number == 40615)
    {
        // Azure SQL firewall error
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Azure SQL firewall blocked the connection. Please add your IP address to the firewall rules.");
        logger.LogWarning("Error: {ErrorMessage}", ex.Message);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
