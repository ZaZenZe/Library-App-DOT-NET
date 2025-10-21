using Library_App.Data;
using Library_App.Services;
using Library_App.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure database context - prioritize environment variables over appsettings.json
var connectionString = Environment.GetEnvironmentVariable("Library_AppContextConnection") 
    ?? builder.Configuration.GetConnectionString("Library_AppContextConnection");

// DEBUG: Log what connection string is being used
Console.WriteLine("========== CONNECTION STRING DEBUG ==========");
Console.WriteLine($"Environment Variable: {Environment.GetEnvironmentVariable("Library_AppContextConnection") ?? "NOT SET"}");
Console.WriteLine($"From Config: {builder.Configuration.GetConnectionString("Library_AppContextConnection") ?? "NOT SET"}");
if (!string.IsNullOrEmpty(connectionString))
{
    var preview = connectionString.Length > 100 ? connectionString.Substring(0, 100) + "..." : connectionString;
    Console.WriteLine($"Final Connection String: {preview}");
}
else
{
    Console.WriteLine("WARNING: Connection string not configured. Database features will not work.");
    Console.WriteLine("Please configure 'Library_AppContextConnection' in Azure App Service Configuration.");
}
Console.WriteLine("============================================");

// Only configure DbContext if connection string exists
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<Library_AppContext>(options =>
        options
     .UseSqlServer(connectionString)
            // Suppress the PendingModelChangesWarning so migrations can apply on an empty DB
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
    );

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
}
else
{
    // Provide dummy DbContext and Identity for when DB is not configured
    // This allows the app to start and show a proper error message
    builder.Services.AddDbContext<Library_AppContext>(options =>
        options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DummyDb;Trusted_Connection=true;MultipleActiveResultSets=true"));
    
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<Library_AppContext>();
}

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

// Run migrations and seed data - only if connection string is configured
var skipMigrations = Environment.GetEnvironmentVariable("SKIP_DB_MIGRATIONS");
if (!string.IsNullOrEmpty(connectionString) && 
 (string.IsNullOrEmpty(skipMigrations) || skipMigrations.ToLower() != "true"))
{
    try
    {
   using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
        var context = services.GetRequiredService<Library_AppContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
      
     logger.LogInformation("Starting database migration...");
        
   // Apply pending migrations
        await context.Database.MigrateAsync();
        
        logger.LogInformation("Database migration completed successfully.");
        
        // Seed initial data
      logger.LogInformation("Starting data seeding...");
        await DbInitializer.SeedAsync(context);
  logger.LogInformation("Data seeding completed successfully.");
    }
    catch (SqlException ex) when (ex.Number == 40615)
    {
        // Azure SQL firewall error
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Azure SQL firewall blocked the connection. Please add your IP address to the firewall rules.");
      logger.LogWarning("Error: {ErrorMessage}", ex.Message);
 logger.LogWarning("To fix this:");
logger.LogWarning("1. Go to Azure Portal ? SQL Database ? Set server firewall");
        logger.LogWarning("2. Add your client IP address");
        logger.LogWarning("3. Or enable 'Allow Azure services and resources to access this server'");
    }
    catch (SqlException ex)
    {
  var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "SQL error occurred during migration or seeding. Error Number: {ErrorNumber}", ex.Number);
        logger.LogError("SQL Error: {SqlError}", ex.Message);
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
