using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class CleanupHistoryConfiguration : IEntityTypeConfiguration<CleanupHistory>
{
    public void Configure(EntityTypeBuilder<CleanupHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CleanupDate).IsRequired();

        builder.HasOne(x => x.Wagon)
            .WithMany()
            .HasForeignKey(x => x.WagonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
