using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Common;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Employees;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Queries.GetDailySchedule;

[Collection(WebAppFactoryCollection.CollectionName)]
public class GetDailyScheduleQueryHandlerTests : IAsyncLifetime
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _context;

    private readonly IServiceScope _scope;
    private readonly WebAppFactory _factory;

    public GetDailyScheduleQueryHandlerTests(WebAppFactory factory)
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
    public async Task Handle_ShouldReturnScheduledResult()
    {
        var ct = CancellationToken.None;

        DateOnly fakeDate = WorkOrderTestHelper.GetTomorrow().AddDays(1);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(fakeDate.ToDateTime(AppSettingsTestData.DefaultOpeningTime), TimeSpan.Zero));

        var today = _factory.FakeTimeProvider.GetUtcNow();

        List<(Spot Spot, List<WorkOrderDto> SpotList)> spotsList =
        [
            (Spot.A,await CreatWorkOrdersListBySpot(Spot.A, 4, today, ct)),
            (Spot.B,await CreatWorkOrdersListBySpot(Spot.B, 2, today, ct)),
            (Spot.C,await CreatWorkOrdersListBySpot(Spot.C, 6, today, ct)),
            (Spot.D,await CreatWorkOrdersListBySpot(Spot.D, 1, today, ct))
        ];

        TimeZoneInfo localZone = TimeZoneInfo.Utc;

        var query = new GetDailyScheduleQuery(localZone, DateOnly.FromDateTime(today.UtcDateTime));


        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var resultValue = result.Value;
        Assert.NotNull(resultValue);
        Assert.Equal(DateOnly.FromDateTime(today.UtcDateTime), resultValue.OnDate);
        Assert.Equal(spotsList.Count, resultValue.Spots.Count);

        foreach (var spot in spotsList)
        {
            Assert.Contains(resultValue.Spots, s => s.Spot == spot.Spot && s.Slots.Where(s => s.IsOccupied).Count() == spot.SpotList.Count);
        }

    }

    [Fact]
    public async Task Handle_ShouldFilterByLaborCorrectly()
    {
        var ct = CancellationToken.None;

        DateOnly fakeDate = WorkOrderTestHelper.GetTomorrow().AddDays(2);

        _factory.FakeTimeProvider.SetUtcNow(new DateTimeOffset(fakeDate.ToDateTime(AppSettingsTestData.DefaultOpeningTime), TimeSpan.Zero));

        var today = _factory.FakeTimeProvider.GetUtcNow();

        var labor1 = EmployeeFactory.CreateLabor().Value;
        var labor2 = EmployeeFactory.CreateLabor().Value;

        List<(Guid LaborId, WorkOrderDto WorkOrders)> labor1List =
        [
           (labor1.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today, hoursOffset: 0, labor: labor1)),
           (labor1.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today, hoursOffset: 1, labor: labor1)),
           (labor1.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.C, startAt: today, hoursOffset: 2, labor: labor1)),
           (labor1.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.B, startAt: today, hoursOffset: 3, labor: labor1)),
        ];

        List<(Guid LaborId, WorkOrderDto WorkOrders)> labor2List =
        [
           (labor2.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.D, startAt: today, hoursOffset: 0, labor: labor2)),
           (labor2.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.D, startAt: today, hoursOffset: 1, labor: labor2)),
           (labor2.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.A, startAt: today, hoursOffset: 2, labor: labor2)),
           (labor2.Id ,await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: Spot.C, startAt: today, hoursOffset: 3, labor: labor2)),
        ];


        TimeZoneInfo localZone = TimeZoneInfo.Utc;

        var query = new GetDailyScheduleQuery(
            localZone,
            DateOnly.FromDateTime(today.UtcDateTime),
            LaborId: labor1.Id);


        var result = await _mediator.Send(query, ct);

        Assert.True(result.IsSuccess);
        var resultValue = result.Value;
        Assert.NotNull(resultValue);
        Assert.Equal(DateOnly.FromDateTime(today.UtcDateTime), resultValue.OnDate);

        var resultByLabor = resultValue.Spots.SelectMany(s => s.Slots).Where(s => s.IsOccupied).GroupBy(s => s.Labor!.LaborId, (laborId, slot) => new { laborId, slotsCount = slot.Count() });
        Assert.Single(resultByLabor);
        Assert.Equal(labor1List.Count, resultByLabor.Single().slotsCount);

    }

    private async Task<List<WorkOrderDto>> CreatWorkOrdersListBySpot(Spot spot, int count, DateTimeOffset date, CancellationToken ct = default, Employee? labor = null)
    {

        List<WorkOrderDto> list = [];

        for (int i = 0; i < count; i++)
        {
            var dto = await WorkOrderTestHelper.CreateValidWorkOrder(_mediator, _context, ct, spot: spot, startAt: date, hoursOffset: i, labor: labor);

            list.Add(dto);
        }

        return list;
    }
}
