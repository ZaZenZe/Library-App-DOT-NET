using System.Text.RegularExpressions;

namespace Library_App.Middleware;

public class IsbnValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IsbnValidationMiddleware> _logger;
    
    // ISBN regex pattern (supports ISBN-10 and ISBN-13)
    private static readonly Regex IsbnPattern = new Regex(
        @"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public IsbnValidationMiddleware(RequestDelegate next, ILogger<IsbnValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // Check if this is an ISBN-related endpoint
        if (!string.IsNullOrEmpty(path) && 
            (path.Contains("/isbn/", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("/import/isbn/", StringComparison.OrdinalIgnoreCase)))
        {
            // Extract ISBN from path (last segment)
            var pathSegments = path.Split('/');
            var isbn = pathSegments.LastOrDefault();

            if (!string.IsNullOrEmpty(isbn))
            {
                // Validate ISBN format
                if (!IsbnPattern.IsMatch(isbn))
                {
                    _logger.LogWarning("Invalid ISBN format detected in request: {Isbn}", isbn);
                    
                    Console.WriteLine($"[IsbnValidation] Invalid ISBN format: {isbn}");

                    // Return 400 Bad Request with error message
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Invalid ISBN format",
                        message = $"The provided ISBN '{isbn}' does not match the expected format.",
                        expectedFormat = "ISBN-10 or ISBN-13 format (e.g., 978-0-123456-78-9 or 0123456789)"
                    });
                    
                    return; // Stop pipeline execution
                }

                _logger.LogInformation("Valid ISBN detected: {Isbn}", isbn);
                Console.WriteLine($"[IsbnValidation] Valid ISBN: {isbn}");
            }
        }

        // Continue with the request pipeline
        await _next(context);
    }
}
