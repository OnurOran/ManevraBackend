using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class WeeklyMaintenanceEntryConfiguration : IEntityTypeConfiguration<WeeklyMaintenanceEntry>
{
    public void Configure(EntityTypeBuilder<WeeklyMaintenanceEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.WeekStartDate).IsRequired();
        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.ShiftType).IsRequired();
        builder.Property(x => x.SlotIndex).IsRequired();
        builder.Property(x => x.TableType).IsRequired().HasDefaultValue(MaintenanceTableType.Bakim);

        // One wagon per cell per table per week
        builder.HasIndex(x => new { x.TableType, x.WeekStartDate, x.DayOfWeek, x.ShiftType, x.SlotIndex })
            .IsUnique();

        builder.HasOne(x => x.Wagon)
            .WithMany()
            .HasForeignKey(x => x.WagonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
