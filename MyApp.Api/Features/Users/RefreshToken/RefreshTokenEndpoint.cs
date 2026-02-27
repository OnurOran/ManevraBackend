using Microsoft.Extensions.Options;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.RefreshToken;

[MapToGroup("users")]
public static class RefreshTokenEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/refresh-token", async Task<IResult> (
            HttpContext httpContext,
            ICommandHandler<RefreshTokenCommand, AuthResponse> handler,
            IOptions<JwtOptions> jwtOptions,
            IWebHostEnvironment env,
            CancellationToken ct) =>
        {
            // The refresh token arrives exclusively via the HttpOnly cookie — no body needed.
            var rawToken = httpContext.Request.Cookies[RefreshTokenCookieHelper.CookieName];
            if (string.IsNullOrEmpty(rawToken))
                return Results.Json(ApiResponse<AuthResponse>.Fail("Refresh token not found."), statusCode: StatusCodes.Status401Unauthorized);

            var command = new RefreshTokenCommand { Token = rawToken };
            var result = await handler.Handle(command, ct);

            if (!result.IsSuccess)
            {
                // Token is invalid or expired — clear the stale cookie so the browser stops sending it.
                RefreshTokenCookieHelper.Clear(httpContext.Response, env.IsDevelopment());
                return Results.Json(ApiResponse<AuthResponse>.Fail(result.Error!), statusCode: StatusCodes.Status401Unauthorized);
            }

            // Rotate: replace old cookie with newly issued refresh token.
            RefreshTokenCookieHelper.Append(httpContext.Response, result.Value!.RefreshToken, jwtOptions.Value.RefreshTokenExpirationDays, env.IsDevelopment());

            return Results.Ok(ApiResponse<AuthResponse>.Ok(result.Value!));
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .AllowAnonymous()
        .RequireRateLimiting("auth");
    }
}
