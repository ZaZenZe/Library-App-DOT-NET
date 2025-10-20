namespace Library_App.Middleware;

public class BooksLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BooksLoggingMiddleware> _logger;

    public BooksLoggingMiddleware(RequestDelegate next, ILogger<BooksLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if path starts with "/books"
        if (context.Request.Path.StartsWithSegments("/books"))
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            
            // Log using ILogger
            _logger.LogInformation("Books API Request: {Method} {Path}", method, path);
            
            // Write to console for visibility
            Console.WriteLine($"[BooksLoggingMiddleware] {method} {path}");
        }

        // Always call next to continue pipeline (non-blocking pattern)
        await _next(context);
    }
}
