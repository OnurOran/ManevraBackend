using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Users;
using FluentValidation;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.RegisterUser;

[MapToGroup("users")]
public static class RegisterUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/register", async Task<IResult> (
            RegisterUserRequest request,
            ICommandHandler<RegisterUserCommand, UserResponse> handler,
            IValidator<RegisterUserCommand> validator,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/users/me", ApiResponse<UserResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<UserResponse>.Fail(result.Error!));
        })
        .WithName("RegisterUser")
        .WithTags("Auth")
        .AllowAnonymous()
        .RequireRateLimiting("auth");
    }
}
