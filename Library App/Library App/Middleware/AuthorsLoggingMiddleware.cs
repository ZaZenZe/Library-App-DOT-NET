namespace Library_App.Middleware;

public class AuthorsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorsLoggingMiddleware> _logger;

    public AuthorsLoggingMiddleware(RequestDelegate next, ILogger<AuthorsLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if path starts with "/authors"
        if (context.Request.Path.StartsWithSegments("/authors"))
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            
            // Log using ILogger
            _logger.LogInformation("Authors API Request: {Method} {Path}", method, path);
            
            // Write to console for visibility
            Console.WriteLine($"[AuthorsLoggingMiddleware] {method} {path}");
        }

        // Always call next to continue pipeline (non-blocking pattern)
        await _next(context);
    }
}
