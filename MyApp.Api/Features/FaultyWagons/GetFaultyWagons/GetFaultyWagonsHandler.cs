using Microsoft.EntityFrameworkCore;
using MyApp.Api.Common.Behaviors;
using MyApp.Api.Common.Models;
using MyApp.Api.Contracts.Manevra;
using MyApp.Api.Domain.Entities.Manevra;
using MyApp.Api.Infrastructure.Persistence;

namespace MyApp.Api.Features.FaultyWagons.GetFaultyWagons;

public class GetFaultyWagonsHandler : IQueryHandler<GetFaultyWagonsQuery, List<FaultyWagonResponse>>
{
    private readonly AppDbContext _db;

    public GetFaultyWagonsHandler(AppDbContext db) => _db = db;

    private static readonly Dictionary<SectionType, string> SectionNames = new()
    {
        [SectionType.BasMakasYonu] = "Baş Makas Yönü",
        [SectionType.ItfaiyeYonu] = "İtfaiye Yönü",
        [SectionType.ItfaiyeTaraf] = "İtfaiye Taraf",
        [SectionType.AtolyeYollari] = "Atölye Yolları",
        [SectionType.YikamaTaraf] = "Yıkama Taraf",
        [SectionType.HazirDiziler] = "Hazır Diziler",
    };

    public async Task<Result<List<FaultyWagonResponse>>> Handle(GetFaultyWagonsQuery query, CancellationToken ct)
    {
        var entries = await _db.FaultyWagonEntries
            .AsNoTracking()
            .Include(e => e.Wagon)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);

        // Get wagon locations (slot → track)
        var wagonIds = entries.Select(e => e.WagonId).ToList();
        var slots = await _db.TrackSlots
            .AsNoTracking()
            .Include(s => s.Track)
            .Where(s => s.WagonId.HasValue && wagonIds.Contains(s.WagonId.Value))
            .ToListAsync(ct);

        var slotByWagon = slots.ToDictionary(s => s.WagonId!.Value);

        var result = entries.Select(e =>
        {
            var location = "Yerleştirilmemiş";
            if (slotByWagon.TryGetValue(e.WagonId, out var slot))
            {
                var sectionName = SectionNames.GetValueOrDefault(slot.SectionType, "Bilinmeyen");
                location = $"{slot.Track.Name}/{sectionName}";
            }

            byte slotIndex = 0;
            if (slotByWagon.TryGetValue(e.WagonId, out var slotRef))
                slotIndex = slotRef.SlotIndex;

            return new FaultyWagonResponse
            {
                Id = e.Id,
                WagonId = e.WagonId,
                WagonNumber = e.Wagon.WagonNumber,
                Line = (byte)e.Wagon.Line,
                Status = (byte)e.Wagon.Status,
                Location = location,
                SlotIndex = slotIndex,
                CreatedAt = e.CreatedAt,
            };
        }).ToList();

        return Result<List<FaultyWagonResponse>>.Success(result);
    }
}
