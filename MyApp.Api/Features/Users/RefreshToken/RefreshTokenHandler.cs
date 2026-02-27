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

namespace MyApp.Api.Features.Users.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenHandler> logger)
    {
        _db = db;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var hashed = _jwtTokenService.HashToken(command.Token);
        var existing = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.HashedToken == hashed, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            _logger.LogWarning("Invalid or expired refresh token used");
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
            return Result<AuthResponse>.Failure("User not found.");

        existing.Revoke();

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await LoadPermissionsAsync(roles, cancellationToken);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissions);

        var (rawRefresh, hashedRefresh) = _jwtTokenService.GenerateRefreshToken();
        var expiry = _jwtTokenService.GetRefreshTokenExpiry();

        var newRefreshToken = RefreshTokenEntity.Create(user.Id, hashedRefresh, expiry);
        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

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
