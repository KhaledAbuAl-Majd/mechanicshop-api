using Docker.DotNet.Models;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders.Billing;

namespace MechanicShop.Api.IntegrationTests.Common;

public static class DatabaseTestExtensions
{
    public static async Task<Customer> SeedCustomerAsync(this IAppDbContext context, CancellationToken ct = default)
    {
        var email = Guid.NewGuid().ToString()[..15] + "@gmail.com";
        var phoneNumber = "+" + string.Join("", Enumerable.Range(1, 11).Select(_ => Random.Shared.Next(1, 10)));

        var customer = CustomerFactory.CreateCustomer(email: email, phoneNumber: phoneNumber, setListIfNull: true).Value;

        context.Customers.Add(customer);

        await context.SaveChangesAsync(ct);

        return customer;
    }

    public static async Task<RepairTask> SeedRepairTaskAsync(this IAppDbContext context, CancellationToken ct = default)
    {
        var partName = $"OilFilter-{Guid.NewGuid().ToString()[..8]}";
        var part = PartFactory.CreatePart(name: partName).Value;

        var taskName = $"OilChange-{Guid.NewGuid().ToString()[..8]}";
        var repairTask = RepairTaskFactory.CreateRepairTask(name: taskName, parts: [part]).Value;

        context.RepairTasks.Add(repairTask);
        await context.SaveChangesAsync(ct);

        return repairTask;
    }

    public static async Task<WorkOrder> SeedWorkOrderAsync(this IAppDbContext context,
        int hoursOffset = 0,
        Spot spot = Spot.C,
        TimeProvider? provider = null,
        WorkOrderState state = WorkOrderState.InProgress,
        CancellationToken ct = default)
    {
        provider ??= TimeProvider.System;

        var customer = await SeedCustomerAsync(context, ct);
        var repairTask = await SeedRepairTaskAsync(context, ct);

        var workOrder = WorkOrderTestDataBuilder.Create(provider)
            .UpdateDate(hoursOffset)
            .AtSpot(spot)
            .WithRepairTasks(repairTask)
            .WithVehicle(customer.Vehicles.First().Id)
            .WithState(state)
            .Build();

        context.WorkOrders.Add(workOrder);

        await context.SaveChangesAsync(ct);

        return workOrder;
    }


    public static async Task<Invoice> SeedInvoiceAsync(
        this IAppDbContext context,
        TimeProvider? provider = null,
        CancellationToken ct = default)
    {
        provider ??= TimeProvider.System;

        var workOrder = await SeedWorkOrderAsync(context, provider: provider,state:WorkOrderState.Completed, ct: ct);

        var invoice = InvoiceFactory.CreateInvoice(
            workOrderId: workOrder.Id,
            datetime: provider).Value;

        context.Invoices.Add(invoice);

        await context.SaveChangesAsync(ct);

        return invoice;
    }
}
