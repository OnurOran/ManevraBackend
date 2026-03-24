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
        // Get the top-2 cleanups per wagon, ordered by insertion (most recent entry first)
        var allCleanups = await _db.CleanupHistories
            .OrderByDescending(c => c.Id)
            .Select(c => new { c.WagonId, c.CleanupDate })
            .ToListAsync(ct);

        var wagonLookup = await _db.Wagons
            .Select(w => new { w.Id, w.WagonNumber, w.Line })
            .ToDictionaryAsync(w => w.Id, ct);

        var grouped = allCleanups
            .GroupBy(c => c.WagonId)
            .Select(g =>
            {
                var top2 = g.Take(2).ToList();
                var wagon = wagonLookup.GetValueOrDefault(g.Key);
                if (wagon is null) return null;
                return new CleanupEntryResponse
                {
                    WagonId = g.Key,
                    WagonNumber = wagon.WagonNumber,
                    Line = (byte)wagon.Line,
                    CleanupDate = top2[0].CleanupDate,
                    PreviousCleanupDate = top2.Count > 1 ? top2[1].CleanupDate : null,
                };
            })
            .Where(e => e is not null)
            .Cast<CleanupEntryResponse>()
            .ToList();

        return Result<List<CleanupEntryResponse>>.Success(grouped);
    }
}
