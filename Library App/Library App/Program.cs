using Library_App.Data;
using Library_App.Services;
using Library_App.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database context - prioritize environment variables over appsettings.json
var connectionString = Environment.GetEnvironmentVariable("Library_AppContextConnection") 
    ?? builder.Configuration.GetConnectionString("Library_AppContextConnection");

// Throw exception if connection string is missing (required for deployment)
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Library_AppContextConnection' not found. " +
        "Please set the environment variable 'Library_AppContextConnection' or add it to appsettings.json.");
}

builder.Services.AddDbContext<Library_AppContext>(options =>
    options.UseSqlServer(connectionString));

// Configure CORS policy for potential frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<Library_AppContext>();

// Register HttpClient for GoogleBooksService
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();

// Register services with dependency injection
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IBookService, BookService>();

// Configure JSON serialization to handle circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add API documentation services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAll");

// Register custom middleware (before authentication)
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<IsbnValidationMiddleware>();
app.UseMiddleware<AuthorsLoggingMiddleware>();
app.UseMiddleware<BooksLoggingMiddleware>();

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapGet("/", () => "Server is running")
    .WithName("HealthCheck")
    .WithOpenApi();

app.MapGet("/ping", () => "pong")
    .WithName("Ping")
    .WithOpenApi();

app.Run();
