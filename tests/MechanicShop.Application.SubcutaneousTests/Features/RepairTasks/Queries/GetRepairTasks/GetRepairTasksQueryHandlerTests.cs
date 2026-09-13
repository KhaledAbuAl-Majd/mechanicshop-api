using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Common;
using MechanicShop.Domain.RepairTasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTasks;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetRepairTasksQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetRepairTasksQueryHandlerTests(WebAppFactory factory)
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
        var ct = CancellationToken.None;

        //var repairTasksBefore = await _context.RepairTasks.ToListAsync(ct);

        var repairTasks = new List<RepairTask>
        {
            await RepairTaskTestHelper.CreateValidRepairTask(_context, ct),
            await RepairTaskTestHelper.CreateValidRepairTask(_context, ct),
            await RepairTaskTestHelper.CreateValidRepairTask(_context, ct),
        };

        var query = new GetRepairTasksQuery();

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        //Assert.Equal(repairTasks.Count, result.Value.Count);

        //foreach (var repairTask in repairTasks)
        //{
        //    Assert.Contains(result.Value, rt => rt.RepairTaskId == repairTask.Id);
        //}
    }
}
