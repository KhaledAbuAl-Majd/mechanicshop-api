using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateWorkOrderRepairTasksCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;
    private readonly IServiceScope _scope;

    private readonly WebAppFactory _factory;

    public UpdateWorkOrderRepairTasksCommandHandlerTests(WebAppFactory factory)
    {
        _factory = factory;
        (_mediator, _context, _scope) = _factory.CreateMediatorAndAppDbContext();
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
    public async Task Handle_ShouldFail_WhenWorkOrderNotFound()
    {
        var ct = CancellationToken.None;
        var command = new UpdateWorkOrderRepairTasksCommand(Guid.NewGuid(), [Guid.NewGuid()]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRepairTaskIdsEmpty()
    {
        var ct = CancellationToken.None;
        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto.WorkOrderId, []);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.AtLeastOneRepairTaskIsRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public async Task Handle_ShouldFail_WhenRepairTaskNotFound(int emptyIdsCount)
    {
        var ct = CancellationToken.None;
        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct);

        List<Guid> repairTaskIds = [];

        var repairTask1 = RepairTaskFactory.CreateRepairTask().Value;
        var repairTask2 = RepairTaskFactory.CreateRepairTask().Value;

        _context.RepairTasks.AddRange(repairTask1, repairTask2);
        await _context.SaveChangesAsync(ct);

        var fakeIds = Enumerable.Range(0, emptyIdsCount).Select(_ => Guid.NewGuid()).ToList();
        repairTaskIds.AddRange(fakeIds);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto.WorkOrderId, [.. repairTaskIds]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOutsideOperatingHours()
    {
        var ct = CancellationToken.None;

        var workPeriod = (AppSettingsTestData.DefaultClosingTime - AppSettingsTestData.DefaultOpeningTime).Hours;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, hoursOffset: workPeriod - 1, spot: Spot.C, cancellationToken: ct);

        var repairTask = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: RepairDurationInMinutes.Min180).Value;

        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(ct);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto.WorkOrderId, [repairTask.Id]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.WorkOrderOutsideOperatingHour(workOrderDto.StartAtUtc, default).Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldFail_WhenUnavailableSpot()
    {
        var ct = CancellationToken.None;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 5);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 4);

        //same spot and diffrent labors
        var repairTask = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: RepairDurationInMinutes.Min90).Value;

        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(ct);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto2.WorkOrderId, [repairTask.Id]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("MechanicShop.Spot.Full", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLaborConflict()
    {
        var ct = CancellationToken.None;

        var labor = EmployeeFactory.CreateLabor().Value;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 5, spot: Spot.D, labor: labor);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 4, spot: Spot.B, labor: labor);

        //diffrent spot but same labor 
        var repairTask = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: RepairDurationInMinutes.Min90).Value;

        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(ct);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto2.WorkOrderId, [repairTask.Id]);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("Labor.Occupied", result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 2, spot: Spot.D);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, hoursOffset: 1, spot: Spot.B);

        //diffrent spots and labors
        var repairTask = RepairTaskFactory.CreateRepairTask(estimatedDurationInMins: RepairDurationInMinutes.Min90).Value;

        _context.RepairTasks.Add(repairTask);
        await _context.SaveChangesAsync(ct);

        var command = new UpdateWorkOrderRepairTasksCommand(workOrderDto2.WorkOrderId, [repairTask.Id]);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        var workOrder = await _context.WorkOrders.Include(wo => wo.RepairTasks)
            .FirstOrDefaultAsync(wo => wo.Id == workOrderDto2.WorkOrderId, ct);

        Assert.NotNull(workOrder);
        Assert.Single(workOrder.RepairTasks);
        Assert.Equal(command.RepairTaskIds[0], workOrder.RepairTasks.First().Id);
    }

}
