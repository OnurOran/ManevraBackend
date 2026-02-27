namespace MyApp.Api.Features._Shared;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("HTTP {Method} {Path} started", context.Request.Method, context.Request.Path);

        await _next(context);

        var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsed);
    }
}
