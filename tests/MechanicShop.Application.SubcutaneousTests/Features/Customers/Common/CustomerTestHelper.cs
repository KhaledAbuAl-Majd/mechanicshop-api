using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Tests.Common.Customers;

namespace MechanicShop.Application.SubcutaneousTests.Features.Customers.Common;

public static class CustomerTestHelper
{
    public static async Task<Customer> CreateValidCustomer(
        IAppDbContext context,
        CancellationToken ct = default,
        List<Vehicle>? vehicles = null)
    {
        var email = Guid.NewGuid().ToString()[..15] + "@gmail.com";
        var phoneNumber = "+" + string.Join("", Enumerable.Range(1, 11).Select(_ => Random.Shared.Next(1, 10)));

        var customer = CustomerFactory.CreateCustomer(email: email, phoneNumber: phoneNumber, vehicles: vehicles, setListIfNull: true).Value;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);

        return customer;
    }

}
