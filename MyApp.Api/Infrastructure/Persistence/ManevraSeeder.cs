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
        var count = 0;

        // ── M1 hattı ────────────────────────────────────────────────────────
        for (var i = 101; i <= 135; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.M1, $"ABB1-{i:D4}")); count++; }

        for (var i = 501; i <= 570; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.M1, $"ABB1-{i:D4}")); count++; }

        // ── T4 hattı — ITA1 serisi ──────────────────────────────────────────
        for (var i = 401; i <= 418; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.T4, $"ITA1-{i:D4}")); count++; }

        // ── T4 hattı — KTA1 serisi ──────────────────────────────────────────
        int[] kta = [203,204,206,207,210,211,213,214,215,216,218,219,220,222,223,225,226,227,228,230,234,236,237,245,247,251,297];
        foreach (var n in kta)
        { db.Wagons.Add(Wagon.Create(n, WagonLine.T4, $"KTA1-{n:D4}")); count++; }

        // ── T4 hattı — RHM1 serisi ──────────────────────────────────────────
        for (var i = 301; i <= 334; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.T4, $"RHM1-{i:D4}")); count++; }

        // ── T1 hattı — ATA1 serisi ──────────────────────────────────────────
        for (var i = 801; i <= 837; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.T1, $"ATA1-{i:D4}")); count++; }

        // ── T1 hattı — BTA1 serisi ──────────────────────────────────────────
        for (var i = 701; i <= 755; i++)
        { db.Wagons.Add(Wagon.Create(i, WagonLine.T1, $"BTA1-{i:D4}")); count++; }

        await db.SaveChangesAsync();
        logger.LogInformation("ManevraSeeder: wagon seeding complete ({Count} wagons).", count);
    }
}
