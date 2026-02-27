using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyApp.Api.Domain.Entities;

namespace MyApp.Api.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.HashedToken).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.HasIndex(x => x.HashedToken);
        builder.HasIndex(x => x.UserId);
    }
}
