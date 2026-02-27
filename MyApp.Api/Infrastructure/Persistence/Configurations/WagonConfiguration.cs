using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class WagonConfiguration : IEntityTypeConfiguration<Wagon>
{
    public void Configure(EntityTypeBuilder<Wagon> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.WagonNumber).IsRequired();
        builder.Property(x => x.Line).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Convoy)
            .WithMany(c => c.Wagons)
            .HasForeignKey(x => x.ConvoyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
