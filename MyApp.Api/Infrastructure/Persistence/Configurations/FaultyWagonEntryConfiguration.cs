using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class FaultyWagonEntryConfiguration : IEntityTypeConfiguration<FaultyWagonEntry>
{
    public void Configure(EntityTypeBuilder<FaultyWagonEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.WagonId).IsUnique();

        builder.HasOne(x => x.Wagon)
            .WithMany()
            .HasForeignKey(x => x.WagonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
