using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class WagonTransferConfiguration : IEntityTypeConfiguration<WagonTransfer>
{
    public void Configure(EntityTypeBuilder<WagonTransfer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.IsApproved).IsRequired();
        builder.Property(x => x.RequestedAt).IsRequired();

        builder.HasOne(x => x.Wagon)
            .WithMany()
            .HasForeignKey(x => x.WagonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FromSlot)
            .WithMany()
            .HasForeignKey(x => x.FromSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToSlot)
            .WithMany()
            .HasForeignKey(x => x.ToSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
