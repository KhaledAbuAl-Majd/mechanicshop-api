using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder.ToTable("WorkOrders");

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.HasKey(wo => wo.Id).IsClustered(false);

            builder.Property(wo => wo.LaborId).IsRequired();

            builder.HasOne(wo => wo.Labor).WithMany().HasForeignKey(wo => wo.LaborId);

            builder.HasOne(wo => wo.Invoice)
                .WithOne(i => i.WorkOrder)
                .HasForeignKey<Invoice>(i => i.WorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(wo => wo.State).HasConversion<string>().IsRequired().HasMaxLength(15);

            builder.Property(wo => wo.StartAtUtc).IsRequired();

            builder.Property(wo => wo.EndAtUtc).IsRequired();

            builder.Property(wo => wo.Tax).HasDefaultValue(0m).HasPrecision(18, 2);

            builder.Property(wo => wo.Discount).HasDefaultValue(0m).HasPrecision(18, 2);

            builder.HasMany(wo => wo.RepairTasks)
                .WithMany()
                .UsingEntity(j => j.ToTable("WorkOrderRepairTasks"));

            builder.Property(wo => wo.VehicleId).IsRequired();

            builder.HasOne(wo => wo.Vehicle).WithMany().HasForeignKey(wo => wo.VehicleId);

            builder.HasIndex(wo => wo.LaborId);

            builder.HasIndex(wo => wo.VehicleId);

            builder.HasIndex(wo => wo.State);

            builder.HasIndex(wo => new { wo.StartAtUtc, wo.EndAtUtc });

            builder.Property(wo => wo.Spot).HasConversion<string>().IsRequired().HasMaxLength(10);
            
            builder.Navigation(wo => wo.RepairTasks).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
