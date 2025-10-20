using System.Diagnostics;

namespace Library_App.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            // Continue with the request pipeline
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // Log the request timing
            _logger.LogInformation(
                "Request {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms",
                method, path, statusCode, elapsedMilliseconds);

            // Write to console for visibility
            Console.WriteLine(
                $"[RequestTiming] {method} {path} - {statusCode} - {elapsedMilliseconds}ms");

            // Warn if request takes too long
            if (elapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                    method, path, elapsedMilliseconds);
            }
        }
    }
}
