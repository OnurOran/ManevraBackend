using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Users;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Users.GetUsers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, PagedResult<UserListItemResponse>>
{
    private readonly AppDbContext _db;

    public GetUsersHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PagedResult<UserListItemResponse>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var q = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(u =>
                u.Email!.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        q = query.SortBy switch
        {
            "email"     => query.SortDesc ? q.OrderByDescending(u => u.Email)     : q.OrderBy(u => u.Email),
            "firstName" => query.SortDesc ? q.OrderByDescending(u => u.FirstName) : q.OrderBy(u => u.FirstName),
            "lastName"  => query.SortDesc ? q.OrderByDescending(u => u.LastName)  : q.OrderBy(u => u.LastName),
            _           => query.SortDesc ? q.OrderByDescending(u => u.CreatedAt) : q.OrderBy(u => u.CreatedAt),
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var page     = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Load roles for this page in a single query — no N+1.
        // GroupBy is done in memory (same pattern as GetRolesHandler) to avoid
        // EF Core translation issues with grouped ToDictionaryAsync.
        var userIds = users.Select(u => u.Id).ToList();

        var userRoleRows = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync(cancellationToken);

        var rolesByUser = userRoleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

        var items = users.Select(u => new UserListItemResponse
        {
            Id        = u.Id,
            Email     = u.Email!,
            FirstName = u.FirstName,
            LastName  = u.LastName,
            AvatarUrl = u.AvatarUrl,
            Roles     = rolesByUser.GetValueOrDefault(u.Id, []),
            CreatedAt = u.CreatedAt,
        }).ToList();

        return Result<PagedResult<UserListItemResponse>>.Success(
            PagedResult<UserListItemResponse>.Create(items, totalCount, page, pageSize));
    }
}
