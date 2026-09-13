using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.HasKey(i => i.Id).IsClustered(false);

            builder.Property(i => i.IssuedAtUtc).IsRequired();

            builder.Property(i => i.DiscountAmount).HasDefaultValue(0m).HasPrecision(18, 2).IsRequired();

            builder.Property(i => i.TaxAmount).HasDefaultValue(0m).HasPrecision(18, 2);

            builder.Property(i => i.PaidAt).IsRequired(false);

            builder.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            builder.Property(i => i.WorkOrderId).IsRequired();

            builder.HasIndex(i => i.WorkOrderId).IsUnique();

            builder.Navigation(i => i.LineItems).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(i => i.LineItems, items =>
            {
                items.ToTable("InvoiceLineItems");

                items.WithOwner().HasForeignKey(i => i.InvoiceId);

                items.HasKey(i => new { i.InvoiceId, i.LineNumber });

                items.Property(i => i.LineNumber).ValueGeneratedNever();

                items.Property(i => i.Description).HasMaxLength(200).IsRequired();

                items.Property(i => i.Quantity).IsRequired();

                items.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
            });

            builder.HasIndex(i => i.Status);
        }
    }
}
