using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTaskByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetRepairTaskByIdQueryHandlerTests(WebAppFactory factory)
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

        var query = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var result = await _mediator.Send(query, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrors.RepairTaskNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task Handle_ShouldSuccess_WhenValidData()
    {
        var ct = CancellationToken.None;

        var expectedRepairTask = await RepairTaskTestHelper.CreateValidRepairTask(_context, ct);

        var query = new GetRepairTaskByIdQuery(expectedRepairTask.Id);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(expectedRepairTask.Id, dto.RepairTaskId);
        Assert.Equal(expectedRepairTask.Name, dto.Name);
        Assert.Equal(expectedRepairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(expectedRepairTask.LaborCost, dto.LaborCost);
        Assert.Equal(expectedRepairTask.Parts.Count, dto.Parts.Count);
    }
}
