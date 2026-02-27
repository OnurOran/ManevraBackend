using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.Wagons.GetWagons;

public class GetWagonsHandler : IQueryHandler<GetWagonsQuery, List<WagonResponse>>
{
    private readonly AppDbContext _db;

    public GetWagonsHandler(AppDbContext db) => _db = db;

    public async Task<Result<List<WagonResponse>>> Handle(GetWagonsQuery query, CancellationToken ct)
    {
        var q = _db.Wagons.AsQueryable();

        if (query.Line.HasValue)
            q = q.Where(w => w.Line == (WagonLine)query.Line.Value);

        // Left join to find which slot each wagon occupies
        var wagons = await q
            .GroupJoin(
                _db.TrackSlots,
                w => w.Id,
                s => s.WagonId,
                (w, slots) => new { Wagon = w, Slots = slots })
            .SelectMany(
                x => x.Slots.DefaultIfEmpty(),
                (x, slot) => new WagonResponse
                {
                    Id = x.Wagon.Id,
                    WagonNumber = x.Wagon.WagonNumber,
                    Line = (byte)x.Wagon.Line,
                    IsOnlyMiddle = x.Wagon.IsOnlyMiddle,
                    Status = (byte)x.Wagon.Status,
                    ConvoyId = x.Wagon.ConvoyId,
                    SlotId = slot != null ? slot.Id : null,
                })
            .OrderBy(w => w.WagonNumber)
            .ToListAsync(ct);

        return Result<List<WagonResponse>>.Success(wagons);
    }
}
