using FluentValidation;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Extensions;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Common.Attributes;

namespace MyApp.Api.Features.Users.UpdateProfile;

[MapToGroup("users")]
public static class UpdateProfileEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/me", async Task<IResult> (
            UpdateProfileRequest request,
            ICommandHandler<UpdateProfileCommand, UserResponse> handler,
            IValidator<UpdateProfileCommand> validator,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
        {
            if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                return Results.Unauthorized();

            var command = new UpdateProfileCommand
            {
                UserId = currentUser.UserId.Value,
                FirstName = request.FirstName,
                LastName = request.LastName,
                AvatarUrl = request.AvatarUrl
            };

            var (isValid, errorResult) = await validator.ValidateRequestAsync(command, ct);
            if (!isValid) return errorResult!;

            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Ok(ApiResponse<UserResponse>.Ok(result.Value!))
                : Results.BadRequest(ApiResponse<UserResponse>.Fail(result.Error!));
        })
        .WithName("UpdateProfile")
        .WithTags("Users")
        .RequireAuthorization();
    }
}
