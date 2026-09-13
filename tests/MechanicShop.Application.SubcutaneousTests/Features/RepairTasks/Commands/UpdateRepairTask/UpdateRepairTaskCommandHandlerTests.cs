using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.UpdateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class UpdateRepairTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public UpdateRepairTaskCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldFail_WhenRepairTaskNotFound()
    {
        var ct = CancellationToken.None;

        var partCommand = new UpdateRepairTaskPartCommand(PartId: Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(
            RepairTaskId: Guid.NewGuid(),
            Name: "chnage oil - 2342z",
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);


        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRepairTaskNameAlreadyExists()
    {
        var ct = CancellationToken.None;

        var repairTask1 = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);
        var repairTask2 = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);


        var partCommand = new UpdateRepairTaskPartCommand(PartId: Guid.NewGuid(), Name: "oilFilter-1234", Cost: 40, Quantity: 3);

        var command = new UpdateRepairTaskCommand(
            RepairTaskId: repairTask2.Id,
            Name: repairTask1.Name,
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);


        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.DuplicateName.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var repairTask = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);

        var partCommand = new UpdateRepairTaskPartCommand(PartId: repairTask.Parts.Single().Id, Name: "oilFilter-12345", Cost: 70, Quantity: 1);

        var command = new UpdateRepairTaskCommand(
            RepairTaskId: repairTask.Id,
            Name: repairTask.Name,
            EstimatedDurationInMins: RepairDurationInMinutes.Min120,
            LaborCost: 50,
            Parts: [partCommand]);


        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);

        var fresh = await _context.RepairTasks.Include(rt => rt.Parts).FirstOrDefaultAsync(rt => rt.Id == command.RepairTaskId, ct);
        Assert.NotNull(fresh);
        Assert.Equal(command.Name, fresh.Name);
        Assert.Equal(command.LaborCost, fresh.LaborCost);
        Assert.Equal(command.EstimatedDurationInMins, fresh.EstimatedDurationInMins);
        Assert.Equal(command.Parts.Count, fresh.Parts.Count);
        Assert.Single(fresh.Parts);
        Assert.Equal(partCommand.Name, fresh.Parts.Single().Name);
        Assert.Equal(partCommand.Cost, fresh.Parts.Single().Cost);
        Assert.Equal(partCommand.Quantity, fresh.Parts.Single().Quantity);
    }
}
