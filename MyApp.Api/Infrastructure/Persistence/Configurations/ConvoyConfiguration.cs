using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities.Manevra;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class ConvoyConfiguration : IEntityTypeConfiguration<Convoy>
{
    public void Configure(EntityTypeBuilder<Convoy> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
