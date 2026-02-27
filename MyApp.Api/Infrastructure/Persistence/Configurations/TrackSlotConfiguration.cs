using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class TrackSlotConfiguration : IEntityTypeConfiguration<TrackSlot>
{
    public void Configure(EntityTypeBuilder<TrackSlot> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SectionType).IsRequired();
        builder.Property(x => x.SlotIndex).IsRequired();

        builder.HasIndex(x => x.WagonId)
            .IsUnique()
            .HasFilter("[WagonId] IS NOT NULL");

        builder.HasOne(x => x.Track)
            .WithMany(t => t.Slots)
            .HasForeignKey(x => x.TrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Wagon)
            .WithMany()
            .HasForeignKey(x => x.WagonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
