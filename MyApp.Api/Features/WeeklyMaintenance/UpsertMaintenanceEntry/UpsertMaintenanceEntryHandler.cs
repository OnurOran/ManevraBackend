using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Hubs;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.WeeklyMaintenance.UpsertMaintenanceEntry;

public class UpsertMaintenanceEntryHandler : ICommandHandler<UpsertMaintenanceEntryCommand, WeeklyMaintenanceEntryResponse>
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public UpsertMaintenanceEntryHandler(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<Result<WeeklyMaintenanceEntryResponse>> Handle(UpsertMaintenanceEntryCommand command, CancellationToken ct)
    {
        var wagon = await _db.Wagons.FindAsync([command.WagonId], ct);
        if (wagon is null)
            return Result<WeeklyMaintenanceEntryResponse>.Failure("Wagon not found.");

        var tableType = (MaintenanceTableType)command.TableType;
        if (!Enum.IsDefined(tableType))
            return Result<WeeklyMaintenanceEntryResponse>.Failure("Invalid TableType.");

        var maxDay = tableType == MaintenanceTableType.Torna ? 4 : 5;
        if (command.DayOfWeek > maxDay)
            return Result<WeeklyMaintenanceEntryResponse>.Failure($"DayOfWeek must be 0-{maxDay}.");

        var shiftType = (ShiftType)command.ShiftType;
        if (!Enum.IsDefined(shiftType))
            return Result<WeeklyMaintenanceEntryResponse>.Failure("Invalid ShiftType.");

        var maxSlot = tableType == MaintenanceTableType.Torna ? 2
            : shiftType == ShiftType.Gunduz ? 2 : 6;
        if (command.SlotIndex < 1 || command.SlotIndex > maxSlot)
            return Result<WeeklyMaintenanceEntryResponse>.Failure($"SlotIndex must be 1-{maxSlot}.");

        if (command.Priority is not null and not 1 and not 2 and not 3)
            return Result<WeeklyMaintenanceEntryResponse>.Failure("Priority must be 1, 2, or 3.");

        // Upsert: find existing entry for this slot or create new
        var existing = await _db.WeeklyMaintenanceEntries.FirstOrDefaultAsync(
            e => e.TableType == tableType
                && e.WeekStartDate == command.WeekStartDate
                && e.DayOfWeek == command.DayOfWeek
                && e.ShiftType == shiftType
                && e.SlotIndex == command.SlotIndex, ct);

        if (existing is not null)
        {
            existing.SetWagonId(command.WagonId);
            existing.SetPriority(command.Priority);
        }
        else
        {
            existing = WeeklyMaintenanceEntry.Create(
                command.WagonId,
                tableType,
                command.WeekStartDate,
                command.DayOfWeek,
                shiftType,
                command.SlotIndex,
                command.Priority);
            _db.WeeklyMaintenanceEntries.Add(existing);
        }

        await _db.SaveChangesAsync(ct);
        await _notifications.SendToGroupAsync("weekly-maintenance", "WeeklyMaintenanceUpdated", new { }, ct);

        return Result<WeeklyMaintenanceEntryResponse>.Success(new WeeklyMaintenanceEntryResponse
        {
            Id = existing.Id,
            WagonId = wagon.Id,
            WagonNumber = wagon.WagonNumber,
            Line = (byte)wagon.Line,
            Status = (byte)wagon.Status,
            TableType = (byte)existing.TableType,
            WeekStartDate = existing.WeekStartDate.ToString("yyyy-MM-dd"),
            DayOfWeek = existing.DayOfWeek,
            ShiftType = (byte)existing.ShiftType,
            SlotIndex = existing.SlotIndex,
            Priority = existing.Priority,
        });
    }
}
