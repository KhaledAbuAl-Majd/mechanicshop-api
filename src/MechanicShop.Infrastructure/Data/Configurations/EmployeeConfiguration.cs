using MechanicShop.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.HasKey(b => b.Id).IsClustered(false);

            builder.Property(b => b.FirstName).IsRequired().HasMaxLength(50);

            builder.Property(b => b.LastName).IsRequired().HasMaxLength(50);

            builder.Property(b => b.Role).HasConversion<string>().IsRequired().HasMaxLength(20);

            builder.HasIndex(b => b.Role);

            builder.HasIndex(b => new { b.FirstName, b.LastName });
        }
    }
}
