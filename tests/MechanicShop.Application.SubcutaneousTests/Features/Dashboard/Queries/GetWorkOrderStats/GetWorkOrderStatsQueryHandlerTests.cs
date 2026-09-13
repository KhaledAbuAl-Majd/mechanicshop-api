using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.Dashboard.Queries.GetWorkOrderStats;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrderStatsQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetWorkOrderStatsQueryHandlerTests(WebAppFactory factory)
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

        _factory.FakeTimeProvider.SetUtcNow(WorkOrderTestHelper.GetTomorrowOpening(DateTime.UtcNow.AddDays(5)));

        var workOrdersList = new List<WorkOrderDto>()
        {
            await WorkOrderTestHelper.CreateValidWorkOrder(_mediator,_context,ct,hoursOffset:1,startAt:_factory.FakeTimeProvider.GetUtcNow()),
            await WorkOrderTestHelper.CreateValidWorkOrder(_mediator,_context,ct,hoursOffset:2,startAt:_factory.FakeTimeProvider.GetUtcNow()),
            await WorkOrderTestHelper.CreateValidWorkOrder(_mediator,_context,ct,hoursOffset:3,startAt:_factory.FakeTimeProvider.GetUtcNow()),
        };

        var tz = _factory.FakeTimeProvider.LocalTimeZone;
        var date = DateOnly.FromDateTime(_factory.FakeTimeProvider.GetUtcNow().UtcDateTime);
        var query = new GetWorkOrderStatsQuery(tz, date);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var dto = result.Value;
        Assert.NotNull(dto);
        Assert.Equal(workOrdersList.Count, dto.Total);
        Assert.Equal(workOrdersList.Count, dto.Scheduled);
        Assert.Equal(0, dto.InProgress);
        Assert.Equal(0, dto.Completed);
        Assert.Equal(0, dto.Cancelled);
        Assert.Equal(date, dto.Date);
    }
}
