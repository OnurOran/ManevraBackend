using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.WeeklyMaintenance.GetWeeklyMaintenance;

public class GetWeeklyMaintenanceHandler : IQueryHandler<GetWeeklyMaintenanceQuery, List<WeeklyMaintenanceEntryResponse>>
{
    private readonly AppDbContext _db;

    public GetWeeklyMaintenanceHandler(AppDbContext db) => _db = db;

    public async Task<Result<List<WeeklyMaintenanceEntryResponse>>> Handle(GetWeeklyMaintenanceQuery query, CancellationToken ct)
    {
        var entries = await _db.WeeklyMaintenanceEntries
            .AsNoTracking()
            .Include(e => e.Wagon)
            .Where(e => e.WeekStartDate == query.WeekStartDate)
            .OrderBy(e => e.DayOfWeek)
            .ThenBy(e => e.ShiftType)
            .ThenBy(e => e.SlotIndex)
            .Select(e => new WeeklyMaintenanceEntryResponse
            {
                Id = e.Id,
                WagonId = e.WagonId,
                WagonNumber = e.Wagon.WagonNumber,
                Line = (byte)e.Wagon.Line,
                Status = (byte)e.Wagon.Status,
                TableType = (byte)e.TableType,
                WeekStartDate = e.WeekStartDate.ToString("yyyy-MM-dd"),
                DayOfWeek = e.DayOfWeek,
                ShiftType = (byte)e.ShiftType,
                SlotIndex = e.SlotIndex,
                Priority = e.Priority,
            })
            .ToListAsync(ct);

        return Result<List<WeeklyMaintenanceEntryResponse>>.Success(entries);
    }
}
