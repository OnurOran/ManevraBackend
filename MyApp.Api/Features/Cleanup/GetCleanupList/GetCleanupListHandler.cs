using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Cleanup.GetCleanupList;

public class GetCleanupListHandler : IQueryHandler<GetCleanupListQuery, List<CleanupEntryResponse>>
{
    private readonly AppDbContext _db;

    public GetCleanupListHandler(AppDbContext db) => _db = db;

    public async Task<Result<List<CleanupEntryResponse>>> Handle(GetCleanupListQuery query, CancellationToken ct)
    {
        // Get the latest cleanup date per wagon, ordered by oldest first
        var entries = await _db.CleanupHistories
            .GroupBy(c => c.WagonId)
            .Select(g => new
            {
                WagonId = g.Key,
                CleanupDate = g.Max(c => c.CleanupDate),
            })
            .Join(_db.Wagons, c => c.WagonId, w => w.Id, (c, w) => new CleanupEntryResponse
            {
                WagonId = c.WagonId,
                WagonNumber = w.WagonNumber,
                Line = (byte)w.Line,
                CleanupDate = c.CleanupDate,
            })
            .OrderBy(c => c.CleanupDate)
            .ToListAsync(ct);

        return Result<List<CleanupEntryResponse>>.Success(entries);
    }
}
