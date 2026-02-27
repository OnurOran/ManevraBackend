using Microsoft.AspNetCore.Identity;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Contracts.Users;

namespace MyApp.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(UserManager<ApplicationUser> userManager, ILogger<GetCurrentUserHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user is null)
        {
            _logger.LogWarning("Authenticated user {UserId} not found in the database. The account may have been deleted.", query.UserId);
            return Result<UserResponse>.Failure("User not found.");
        }

        return Result<UserResponse>.Success(UserMapper.ToResponse(user));
    }
}
