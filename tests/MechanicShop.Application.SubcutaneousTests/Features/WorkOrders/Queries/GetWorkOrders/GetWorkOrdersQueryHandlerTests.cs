using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.WorkOrders.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetWorkOrders;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetWorkOrdersQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetWorkOrdersQueryHandlerTests(WebAppFactory factory)
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


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Handle_ShouldReturnPaginatedAndProjectedData(int pageNumber)
    {
        var ct = CancellationToken.None;

        DateOnly fakeDate = WorkOrderTestHelper.GetTomorrow().AddDays(10 + pageNumber);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(fakeDate.ToDateTime(AppSettingsTestData.DefaultOpeningTime), TimeSpan.Zero));

        var today = _factory.FakeTimeProvider.GetUtcNow();

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.B, startAt: today);
        var workOrderDto3 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.C, startAt: today);

        int totalCount = 3;
        int pageSize = 2;
        bool hasNextPage = (pageSize * pageNumber) < totalCount;
        bool hasPreviousPage = pageNumber > 1;
        int offset = (pageNumber - 1) * pageSize;
        int currentPageCount = hasNextPage ? pageSize : totalCount - offset;

        var query = new GetWorkOrdersQuery(
            Page: pageNumber,
            PageSize: pageSize,
            StartDateFrom: today.UtcDateTime,
            EndDateFrom: today.UtcDateTime,
            SearchTerm: string.Empty);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);

        var resultValue = result.Value;
        Assert.NotNull(resultValue);
        Assert.Equal(query.Page, resultValue.Page);
        Assert.Equal(query.PageSize, resultValue.PageSize);
        Assert.Equal(totalCount, resultValue.TotalCount);
        Assert.Equal(hasNextPage, resultValue.HasNextPage);
        Assert.Equal(hasPreviousPage, resultValue.HasPreviousPage);
        Assert.Equal(currentPageCount, resultValue.Items.Count);

        var firstItem = resultValue.Items[0];
        Assert.NotNull(firstItem.Vehicle);
        Assert.NotEmpty(firstItem.Customer!);
        Assert.NotEmpty(firstItem.RepairTasks);
    }


    [Fact]
    public async Task Handle_ShouldFilterBySpotCorrectly()
    {
        var ct = CancellationToken.None;

        DateOnly fakeDate = WorkOrderTestHelper.GetTomorrow().AddDays(2);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(fakeDate.ToDateTime(AppSettingsTestData.DefaultOpeningTime), TimeSpan.Zero));

        var today = _factory.FakeTimeProvider.GetUtcNow();

        var workOrderDto1 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today, hoursOffset: 0);
        var workOrderDto2 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today, hoursOffset: 2);
        var workOrderDto3 = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.C, startAt: today);

        int pageSize = 10;


        var query = new GetWorkOrdersQuery(
            Page: 1,
            PageSize: pageSize,
            StartDateFrom: today.UtcDateTime,
            EndDateFrom: today.UtcDateTime,
            SearchTerm: string.Empty,
            Spot: Spot.A);

        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var resultValue = result.Value;
        Assert.NotNull(resultValue);
        Assert.Equal(query.Page, resultValue.Page);
        Assert.Equal(query.PageSize, resultValue.PageSize);
        Assert.Equal(2, resultValue.TotalCount);
        Assert.Equal(2, resultValue.Items.Count);
    }
}
