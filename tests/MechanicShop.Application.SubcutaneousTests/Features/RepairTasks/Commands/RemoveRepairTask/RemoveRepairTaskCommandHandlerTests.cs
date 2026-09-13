using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Tests.Common.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Commands.RemoveRepairTask;

[Collection(WebAppFactoryCollection.CollectionName)]
public class RemoveRepairTaskCommandHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public RemoveRepairTaskCommandHandlerTests(WebAppFactory factory)
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

        var command = new RemoveRepairTaskCommand(Guid.NewGuid());

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRepairTaskAssociatedWithWorkOrders()
    {
        var ct = CancellationToken.None;

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        var workOrderDto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, repairTask: repairTask);

        var command = new RemoveRepairTaskCommand(repairTask.Id);

        var result = await _mediator.Send(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepairTaskErrors.InUse.Code, result.TopError.Code);
    }


    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var repairTask = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);

        var command = new RemoveRepairTaskCommand(repairTask.Id);

        var result = await _mediator.Send(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Deleted, result.Value);

        var exists = await _context.RepairTasks.AnyAsync(rt => rt.Id == repairTask.Id, ct);
        Assert.False(exists);
    }
}
