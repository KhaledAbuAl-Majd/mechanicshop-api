using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.CreateRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class CreateRepairTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public CreateRepairTaskCommandHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldFail_WhenRepairTaskNameAlreadyExists()
    {
        var ct = CancellationToken.None;

        var repairTask = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);
        var command = new CreateRepairTaskCommand(
            Name: repairTask.Name,
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

        var partCommand = new CreateRepairTaskPartCommand(Name: "oilFilter-1234", Cost: 40, Quantity: 3);
        var command = new CreateRepairTaskCommand(
            Name: "chnage oil - 2342z",
            EstimatedDurationInMins: RepairDurationInMinutes.Min30,
            LaborCost: 30,
            Parts: [partCommand]);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var repairTasks = await _context.RepairTasks.Include(rt => rt.Parts)
            .FirstOrDefaultAsync(rt => rt.Id == result.Value.RepairTaskId, ct);

        Assert.NotNull(repairTasks);
        Assert.Equal(command.Name, repairTasks.Name);
        Assert.Equal(command.LaborCost, repairTasks.LaborCost);
        Assert.Equal(command.EstimatedDurationInMins, repairTasks.EstimatedDurationInMins);
        Assert.Equal(command.Parts.Count, repairTasks.Parts.Count);
        Assert.Single(repairTasks.Parts);
        Assert.Equal(partCommand.Name, repairTasks.Parts.Single().Name);
        Assert.Equal(partCommand.Cost, repairTasks.Parts.Single().Cost);
        Assert.Equal(partCommand.Quantity, repairTasks.Parts.Single().Quantity);
    }
}
