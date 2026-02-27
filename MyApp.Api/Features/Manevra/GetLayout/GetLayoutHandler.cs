using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Manevra.GetLayout;

public class GetLayoutHandler : IQueryHandler<GetLayoutQuery, LayoutResponse>
{
    private readonly AppDbContext _db;

    public GetLayoutHandler(AppDbContext db) => _db = db;

    public async Task<Result<LayoutResponse>> Handle(GetLayoutQuery query, CancellationToken ct)
    {
        var tracks = await _db.Tracks
            .Include(t => t.Slots)
                .ThenInclude(s => s.Wagon)
            .OrderBy(t => t.Zone)
            .ThenBy(t => t.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var pendingTransfers = await _db.WagonTransfers
            .Include(wt => wt.Wagon)
            .Where(wt => !wt.IsApproved)
            .AsNoTracking()
            .ToListAsync(ct);

        var pendingDtos = pendingTransfers.Select(pt => new PendingTransferResponse
        {
            Id = pt.Id,
            WagonId = pt.WagonId,
            WagonNumber = pt.Wagon.WagonNumber,
            FromSlotId = pt.FromSlotId,
            ToSlotId = pt.ToSlotId,
            RequestedAt = pt.RequestedAt,
        }).ToList();

        // Build a lookup for pending transfers by slot
        var pendingBySlot = pendingDtos
            .SelectMany(pt => new[] { (SlotId: pt.FromSlotId, Transfer: pt), (SlotId: pt.ToSlotId, Transfer: pt) })
            .GroupBy(x => x.SlotId)
            .ToDictionary(g => g.Key, g => g.First().Transfer);

        var trackDtos = tracks.Select(t => new TrackLayoutResponse
        {
            Id = t.Id,
            Name = t.Name,
            Zone = (byte)t.Zone,
            Slots = t.Slots.OrderBy(s => s.SectionType).ThenBy(s => s.SlotIndex).Select(s => new TrackSlotResponse
            {
                Id = s.Id,
                TrackId = s.TrackId,
                SectionType = (byte)s.SectionType,
                SlotIndex = s.SlotIndex,
                Wagon = s.Wagon is not null ? new WagonResponse
                {
                    Id = s.Wagon.Id,
                    WagonNumber = s.Wagon.WagonNumber,
                    Line = (byte)s.Wagon.Line,
                    IsOnlyMiddle = s.Wagon.IsOnlyMiddle,
                    Status = (byte)s.Wagon.Status,
                    ConvoyId = s.Wagon.ConvoyId,
                    SlotId = s.Id,
                } : null,
                PendingTransfer = pendingBySlot.GetValueOrDefault(s.Id),
            }).ToList(),
        }).ToList();

        return Result<LayoutResponse>.Success(new LayoutResponse
        {
            Tracks = trackDtos,
            PendingTransfers = pendingDtos,
        });
    }
}
