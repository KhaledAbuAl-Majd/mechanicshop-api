using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Customer> Customers { get; }
        DbSet<Part> Parts { get; }
        DbSet<RepairTask> RepairTasks { get; }
        DbSet<Vehicle> Vehicles { get; }
        DbSet<WorkOrder> WorkOrders { get; }
        DbSet<Employee> Employees { get; }
        DbSet<Invoice> Invoices { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
