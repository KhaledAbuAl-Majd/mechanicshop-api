using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.CreateWorkOrder;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateWorkOrderCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator = default!;
    private readonly IAppDbContext _context = default!;
    private readonly IServiceScope _scope = default!;

    private readonly WebAppFactory _factory;

    public CreateWorkOrderCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;
        (_mediator, _context, _scope) = factory.CreateMediatorAndAppDbContext();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.Employees.Add(labor);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(6);

        var spot = Spot.D;
        var command = new CreateWorkOrderCommand(spot, vehicle.Id, scheduledAt, [repairTask.Id], labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.True(result.IsSuccess);
        var value = result.Value;
        Assert.NotNull(value);
        Assert.Equal(vehicle.Id, value.Vehicle!.VehicleId);
        Assert.Equal(spot, value.Spot);
        Assert.Equal(scheduledAt, value.StartAtUtc);
        Assert.Single(value.RepairTasks);
        Assert.Equal(repairTask.Id, value.RepairTasks[0].RepairTaskId);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRepairTaskNotFound()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.Employees.Add(labor);
        await _context.SaveChangesAsync(cancellationToken);

        var fakeRepairTaskId = Guid.NewGuid();
        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening();

        var command = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [fakeRepairTaskId], labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Theory]
    [MemberData(nameof(GetInvalidStartAt))]
    public async Task Handle_ShouldFail_WhenOutsideOperatingHours(DateTimeOffset scheduledAt)
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.Employees.Add(labor);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);


        var command = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHour(command.StartAt, default).Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenShourtDuration()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: RepairDurationInMinutes.Min15).Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.Employees.Add(labor);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(1);

        var command = new CreateWorkOrderCommand(Spot.A, vehicle.Id, scheduledAt, [repairTask.Id], labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUnavailableSpot()
    {
        var cancellationToken = CancellationToken.None;

        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var labor1 = EmployeeFactory.CreateLabor().Value;
        var labor2 = EmployeeFactory.CreateLabor().Value;

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Vehicles.Add(vehicle1);
        _context.Vehicles.Add(vehicle2);
        _context.Customers.Add(customer);

        _context.Employees.Add(labor1);
        _context.Employees.Add(labor2);

        _context.RepairTasks.Add(repairTask);

        await _context.SaveChangesAsync(cancellationToken);

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(2);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], labor1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.A, vehicle2.Id, scheduledAt, [repairTask.Id], labor2.Id);

        await _mediator.Send(command1, cancellationToken);
        var result = await _mediator.Send(command2, cancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVehicleNotFound()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Customers.Add(customer);
        _context.Employees.Add(labor);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var fakeVehilceId = Guid.NewGuid();

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(4);

        var command = new CreateWorkOrderCommand(Spot.A, fakeVehilceId, scheduledAt, [repairTask.Id], labor.Id);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.VehicleNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborNotFound()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var fakeLaborId = Guid.NewGuid();

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(1);

        var command = new CreateWorkOrderCommand(Spot.C, vehicle.Id, scheduledAt, [repairTask.Id], fakeLaborId);

        var result = await _mediator.Send(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.LaborNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenVehicleConflict()
    {
        var cancellationToken = CancellationToken.None;

        var customer = CustomerFactory.CreateCustomer().Value;
        var vehicle = customer.Vehicles.First();

        var labor1 = EmployeeFactory.CreateLabor().Value;
        var labor2 = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        _context.Employees.Add(labor1);
        _context.Employees.Add(labor2);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(4);

        var command1 = new CreateWorkOrderCommand(Spot.C, vehicle.Id, scheduledAt, [repairTask.Id], labor1.Id);
        var command2 = new CreateWorkOrderCommand(Spot.D, vehicle.Id, scheduledAt, [repairTask.Id], labor2.Id);

        await _mediator.Send(command1, cancellationToken);
        var result = await _mediator.Send(command2, cancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborConflict()
    {
        var cancellationToken = CancellationToken.None;

        var vehicle1 = VehicleFactory.CreateVehicle().Value;
        var vehicle2 = VehicleFactory.CreateVehicle().Value;
        var customer = CustomerFactory.CreateCustomer(vehicles: [vehicle1, vehicle2]).Value;

        var labor = EmployeeFactory.CreateLabor().Value;
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        _context.Vehicles.Add(vehicle1);
        _context.Vehicles.Add(vehicle2);
        _context.Customers.Add(customer);

        _context.Employees.Add(labor);
        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(cancellationToken);

        var scheduledAt = WorkOrderTestHelper.GetTomorrowOpening().AddHours(5);

        var command1 = new CreateWorkOrderCommand(Spot.A, vehicle1.Id, scheduledAt, [repairTask.Id], labor.Id);
        var command2 = new CreateWorkOrderCommand(Spot.B, vehicle2.Id, scheduledAt, [repairTask.Id], labor.Id);

        await _mediator.Send(command1, cancellationToken);
        var result = await _mediator.Send(command2, cancellationToken);

        Assert.False(result.IsSuccess);
    }

    public static TheoryData<DateTimeOffset> GetInvalidStartAt => new TheoryData<DateTimeOffset>()
    {
        new DateTimeOffset(WorkOrderTestHelper.GetTomorrow().ToDateTime(AppSettingsTestData.DefaultOpeningTime.AddHours(-1)),TimeSpan.Zero),
        new DateTimeOffset(WorkOrderTestHelper.GetTomorrow().ToDateTime(AppSettingsTestData.DefaultClosingTime.AddHours(1)),TimeSpan.Zero),
    };
}
