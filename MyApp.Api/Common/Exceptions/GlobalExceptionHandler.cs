using Microsoft.AspNetCore.Diagnostics;
using MyApp.Api.Common.Models;

namespace MyApp.Api.Common.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            DomainException ex => (StatusCodes.Status400BadRequest, ApiResponse.Fail(ex.Message)),
            NotFoundException ex => (StatusCodes.Status404NotFound, ApiResponse.Fail(ex.Message)),
            ValidationException ex => (StatusCodes.Status400BadRequest, ApiResponse.Fail(ex.Errors)),
            _ => (StatusCodes.Status500InternalServerError, ApiResponse.Fail("An unexpected error occurred."))
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
