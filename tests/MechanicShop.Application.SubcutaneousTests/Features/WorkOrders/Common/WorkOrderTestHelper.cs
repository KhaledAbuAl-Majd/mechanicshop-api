using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;

public static class WorkOrderTestHelper
{
    public static async Task<WorkOrderDto> CreateValidWorkOrder(
        IMediator mediator,
        IAppDbContext context,
        CancellationToken cancellationToken = default,
        int hoursOffset = 0,
        Spot spot = Spot.D,
        Customer? customer = null,
        Employee? labor = null,
        DateTimeOffset? startAt = null,
        RepairTask? repairTask = null)
    {

        customer ??= CustomerFactory.CreateCustomer(email: $"{Guid.NewGuid().ToString()[..10]}@gmail.com").Value;
        labor ??= EmployeeFactory.CreateLabor().Value;
        repairTask ??= RepairTaskFactory.CreateRepairTask().Value;

        if (context.Customers.Entry(customer).State == EntityState.Detached)
        {
            context.Customers.Add(customer);
        }

        if (context.Employees.Entry(labor).State == EntityState.Detached)
        {
            context.Employees.Add(labor);
        }

        if (context.RepairTasks.Entry(repairTask).State == EntityState.Detached)
        {
            context.RepairTasks.Add(repairTask);
        }

        foreach (var v in customer.Vehicles)
        {
            if (context.Vehicles.Entry(v).State == EntityState.Detached)
            {
                context.Vehicles.Add(v);
            }
        }

        var vehicle = customer.Vehicles?.First();

        await context.SaveChangesAsync(cancellationToken);

        var scheduledAt = startAt ?? GetTomorrowOpening();
        scheduledAt = scheduledAt.AddHours(hoursOffset);

        var command = new CreateWorkOrderCommand(spot, vehicle!.Id, scheduledAt, [repairTask.Id], labor.Id);

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"CreateValidWorkOrder failed with error: {result.TopError.Code} - {result.TopError.Description}");
        }

        return result.Value;
    }

    public static DateTimeOffset GetTomorrowOpening()
    {
        var tomorrow = GetTomorrow();

        return new DateTimeOffset(
        tomorrow.ToDateTime(AppSettingsTestData.DefaultOpeningTime),
        TimeSpan.Zero);
    }

    public static DateOnly GetTomorrow() => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    public static DateTimeOffset GetTomorrowOpening(DateTime today)
    {
        var tomorrow = GetTomorrow(today);

        return new DateTimeOffset(
        tomorrow.ToDateTime(AppSettingsTestData.DefaultOpeningTime),
        TimeSpan.Zero);
    }
    public static DateOnly GetTomorrow(DateTime today) => DateOnly.FromDateTime(today).AddDays(1);
}
