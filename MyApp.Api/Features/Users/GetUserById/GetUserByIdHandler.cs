using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Users.GetUserById;

public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserListItemResponse>
{
    private readonly AppDbContext _db;

    public GetUserByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UserListItemResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
            return Result<UserListItemResponse>.Failure("User not found.");

        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
            .ToListAsync(cancellationToken);

        return Result<UserListItemResponse>.Success(new UserListItemResponse
        {
            Id        = user.Id,
            Email     = user.Email!,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Roles     = roles,
            CreatedAt = user.CreatedAt,
        });
    }
}
