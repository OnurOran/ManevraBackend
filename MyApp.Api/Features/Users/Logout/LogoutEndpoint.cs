using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.Logout;

[MapToGroup("users")]
public static class LogoutEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/logout", async Task<IResult> (
            HttpContext httpContext,
            AppDbContext db,
            JwtTokenService jwtTokenService,
            IWebHostEnvironment env,
            CancellationToken ct) =>
        {
            // Revoke the refresh token in the database if the cookie is present.
            // This prevents the token being usable even before it naturally expires.
            var rawToken = httpContext.Request.Cookies[RefreshTokenCookieHelper.CookieName];
            if (!string.IsNullOrEmpty(rawToken))
            {
                var hashed = jwtTokenService.HashToken(rawToken);
                var token = await db.RefreshTokens
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(t => t.HashedToken == hashed, ct);

                if (token?.IsActive == true)
                {
                    token.Revoke();
                    await db.SaveChangesAsync(ct);
                }
            }

            // Always clear the cookie regardless of whether we found the token in the DB.
            RefreshTokenCookieHelper.Clear(httpContext.Response, env.IsDevelopment());

            return Results.Ok(ApiResponse<bool>.Ok(true));
        })
        .WithName("Logout")
        .WithTags("Auth")
        .RequireAuthorization();
    }
}
