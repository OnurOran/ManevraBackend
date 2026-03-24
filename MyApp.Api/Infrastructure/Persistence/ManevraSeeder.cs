using Microsoft.EntityFrameworkCore;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence;

/// <summary>
/// Seeds tracks and slots for the Manevra system. Idempotent — safe to call on every startup.
/// </summary>
public static class ManevraSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ManevraSeeder));
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Tracks.AnyAsync())
        {
            logger.LogDebug("ManevraSeeder: tracks already exist. Skipping.");
            return;
        }

        logger.LogInformation("ManevraSeeder: seeding tracks and slots...");

        // ── Zone 1: Garaj Yollari (G1-G14) ──────────────────────────────────
        for (var i = 1; i <= 14; i++)
        {
            var track = Track.Create($"G{i}", TrackZone.Garaj);
            db.Tracks.Add(track);
            await db.SaveChangesAsync(); // get track.Id

            // Bas Makas Yonu: 4 slots each
            for (byte s = 1; s <= 4; s++)
                db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.BasMakasYonu, s));

            // Itfaiye Yonu: 4 slots each, except G13 = 8 slots
            var itfaiyeCount = i == 13 ? 8 : 4;
            for (byte s = 1; s <= itfaiyeCount; s++)
                db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.ItfaiyeYonu, s));
        }

        // ── Zone 2: Atolye / TEM / Yikama Yollari ───────────────────────────
        // TEM track
        var tem = Track.Create("TEM", TrackZone.Atolye);
        db.Tracks.Add(tem);
        await db.SaveChangesAsync();

        for (byte s = 1; s <= 4; s++)
            db.TrackSlots.Add(TrackSlot.Create(tem.Id, SectionType.AtolyeYollari, s));

        // TEM also has Itfaiye Taraf: 2 slots
        for (byte s = 1; s <= 2; s++)
            db.TrackSlots.Add(TrackSlot.Create(tem.Id, SectionType.ItfaiyeTaraf, s));

        // TEM also has Yikama Taraf: 2 slots
        for (byte s = 1; s <= 2; s++)
            db.TrackSlots.Add(TrackSlot.Create(tem.Id, SectionType.YikamaTaraf, s));

        // A1-A9 tracks
        for (var i = 1; i <= 9; i++)
        {
            var track = Track.Create($"A{i}", TrackZone.Atolye);
            db.Tracks.Add(track);
            await db.SaveChangesAsync();

            // Itfaiye Taraf: 2 slots, but A6 and A9 are closed (no rows)
            if (i != 6 && i != 9)
            {
                for (byte s = 1; s <= 2; s++)
                    db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.ItfaiyeTaraf, s));
            }

            // Atolye Yollari: TEM and A1-A6 = 4 slots, A7/A8 = 1 slot at index 1, A9 = 1 slot at index 4
            if (i <= 6)
            {
                for (byte s = 1; s <= 4; s++)
                    db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.AtolyeYollari, s));
            }
            else
            {
                byte slotIndex = i == 9 ? (byte)4 : (byte)1;
                db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.AtolyeYollari, slotIndex));
            }

            // Yikama Taraf: 2 slots, but A7 and A8 are closed (no rows)
            if (i != 7 && i != 8)
            {
                for (byte s = 1; s <= 2; s++)
                    db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.YikamaTaraf, s));
            }
        }

        // ── Zone 3: Cari Hatta Hazir Diziler (Sira 1-26) ────────────────────
        for (var i = 1; i <= 26; i++)
        {
            var track = Track.Create($"Sıra {i}", TrackZone.CariHattaHazirDiziler);
            db.Tracks.Add(track);
            await db.SaveChangesAsync();

            for (byte s = 1; s <= 4; s++)
                db.TrackSlots.Add(TrackSlot.Create(track.Id, SectionType.HazirDiziler, s));
        }

        await db.SaveChangesAsync();
        logger.LogInformation("ManevraSeeder: track/slot seeding complete.");

        await SeedWagonsAsync(db, logger);
    }

    private static async Task SeedWagonsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Wagons.AnyAsync())
        {
            logger.LogDebug("ManevraSeeder: wagons already exist. Skipping.");
            return;
        }

        logger.LogInformation("ManevraSeeder: seeding wagons...");

        // M1 hattı — 1xx serisi (IsOnlyMiddle=TRUE) ve 5xx serisi (IsOnlyMiddle=FALSE)
        int[] m1Series1 = [101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120];
        int[] m1Series5 = [501, 502, 503, 504, 505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520];

        foreach (var num in m1Series1)
            db.Wagons.Add(Wagon.Create(num, WagonLine.M1));
        foreach (var num in m1Series5)
            db.Wagons.Add(Wagon.Create(num, WagonLine.M1));

        // Tramvay hattı — 1xx serisi ve 5xx serisi
        int[] tramSeries1 = [131, 132, 133, 134, 135, 136, 137, 138, 139, 140];
        int[] tramSeries5 = [531, 532, 533, 534, 535, 536, 537, 538, 539, 540];

        foreach (var num in tramSeries1)
            db.Wagons.Add(Wagon.Create(num, WagonLine.Tramvay));
        foreach (var num in tramSeries5)
            db.Wagons.Add(Wagon.Create(num, WagonLine.Tramvay));

        await db.SaveChangesAsync();
        logger.LogInformation("ManevraSeeder: wagon seeding complete ({Count} wagons).",
            m1Series1.Length + m1Series5.Length + tramSeries1.Length + tramSeries5.Length);
    }
}
