using FluentValidation;
using Microsoft.Extensions.Options;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.LoginUser;

[MapToGroup("users")]
public static class LoginUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/login", async Task<IResult> (
            LoginRequest request,
            ICommandHandler<LoginUserCommand, AuthResponse> handler,
            IValidator<LoginUserCommand> validator,
            IOptions<JwtOptions> jwtOptions,
            IWebHostEnvironment env,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var command = new LoginUserCommand { Email = request.Email, Password = request.Password };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            if (!result.IsSuccess)
                return Results.Json(ApiResponse<AuthResponse>.Fail(result.Error!), statusCode: StatusCodes.Status401Unauthorized);

            // Write the refresh token into an HttpOnly cookie — it never appears in the response body.
            RefreshTokenCookieHelper.Append(httpContext.Response, result.Value!.RefreshToken, jwtOptions.Value.RefreshTokenExpirationDays, env.IsDevelopment());

            // AuthResponse.RefreshToken is [JsonIgnore] so only AccessToken + AccessTokenExpiresAt reach the client.
            return Results.Ok(ApiResponse<AuthResponse>.Ok(result.Value!));
        })
        .WithName("LoginUser")
        .WithTags("Auth")
        .AllowAnonymous()
        .RequireRateLimiting("auth");
    }
}
