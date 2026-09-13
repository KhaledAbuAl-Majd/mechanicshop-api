using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.HasKey(rt => rt.Id).IsClustered(false);

            builder.Property(rt => rt.TokenHash).HasMaxLength(100).IsUnicode(false);

            builder.HasIndex(rt => rt.TokenHash).IsUnique();

            builder.Property(rt => rt.UserId).IsRequired();

            builder.HasIndex(rt => rt.UserId);

            builder.Property(rt => rt.ExpiresOnUtc).IsRequired();

            builder.Property(rt => rt.RevokedAt).IsRequired(false);
        }
    }
}
