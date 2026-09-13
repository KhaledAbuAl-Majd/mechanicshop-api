using MechanicShop.Domain.Customers.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.HasKey(v => v.Id).IsClustered(false);

            builder.Property(v => v.Make).IsRequired().HasMaxLength(100);

            builder.Property(v => v.Model).IsRequired().HasMaxLength(100);

            builder.Property(v => v.Year).IsRequired();

            builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);

            builder.HasOne(v => v.Customer).WithMany(c => c.Vehicles).HasForeignKey(v => v.CustomerId);

            builder.HasIndex(v => v.LicensePlate).IsUnique();

            builder.HasIndex(v => v.CustomerId);
        }
    }
}
