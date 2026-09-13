using MechanicShop.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id).IsClustered(false);

        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.Type).HasMaxLength(500).IsRequired();

        builder.Property(o => o.Content).IsRequired();

        builder.Property(o => o.OccurredOnUtc).IsRequired();

        builder.Property(o => o.LastModifyOnUtc).IsRequired();

        builder.Property(o => o.ProcessedOnUtc).IsRequired(false);

        builder.HasIndex(o => o.OccurredOnUtc).IsClustered();

        builder.HasIndex(o => o.ProcessedOnUtc)
               .IncludeProperties(o => o.RetryCount)
               .HasFilter("[ProcessedOnUtc] IS NULL");
    }
}
