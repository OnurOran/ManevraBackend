using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Common.Services;
using MyApp.Api.Domain.Entities;
using MyApp.Api.Infrastructure.Persistence;
using MyApp.Api.Contracts.Users;
using RefreshTokenEntity = MyApp.Api.Domain.Entities.RefreshToken;

namespace MyApp.Api.Features.Users.LoginUser;

public class LoginUserHandler : ICommandHandler<LoginUserCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AppDbContext _db;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwtTokenService,
        AppDbContext db,
        IOptions<JwtOptions> jwtOptions,
        ILogger<LoginUserHandler> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _db = db;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", command.Email);
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Locked out user {UserId} attempted to log in", user.Id);
            return Result<AuthResponse>.Failure("Account is temporarily locked. Please try again later.");
        }

        if (!await _userManager.CheckPasswordAsync(user, command.Password))
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Failed login attempt for user {UserId}", user.Id);
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await LoadPermissionsAsync(roles, cancellationToken);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions);

        var (rawRefresh, hashedRefresh) = _jwtTokenService.GenerateRefreshToken();
        var expiry = _jwtTokenService.GetRefreshTokenExpiry();

        var refreshToken = RefreshTokenEntity.Create(user.Id, hashedRefresh, expiry);
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefresh,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes)
        });
    }

    private async Task<List<string>> LoadPermissionsAsync(IList<string> roleNames, CancellationToken ct)
    {
        if (roleNames.Count == 0) return [];

        var roleIds = await _db.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync(ct);

        return await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync(ct);
    }
}
