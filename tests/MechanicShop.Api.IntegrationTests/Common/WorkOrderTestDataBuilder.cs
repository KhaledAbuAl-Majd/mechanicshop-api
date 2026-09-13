using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Security;

namespace MechanicShop.Api.IntegrationTests.Common;

public interface ITestDataBuilder<T>
{
    T Build();
}
public class WorkOrderTestDataBuilder : ITestDataBuilder<WorkOrder>
{
    private TimeProvider _provider = TimeProvider.System;

    private Guid _id = Guid.NewGuid();
    private Guid _vehicleId = Guid.NewGuid();
    private DateTimeOffset _startAt = DateTimeOffset.UtcNow;
    private DateTimeOffset _endAt = DateTimeOffset.UtcNow.AddHours(2);
    private Guid _laborId = Guid.Parse(TestUsers.Labor01.Id);
    private Spot _spot = Spot.A;
    private List<RepairTask> _repairTasks = [];
    private WorkOrderState _state = WorkOrderState.Scheduled;

    public static WorkOrderTestDataBuilder Create() => new()
    {
        _startAt = GetOpening(DateTime.UtcNow.AddDays(1)),
        _endAt = GetOpening(DateTime.UtcNow.AddDays(1)).AddHours(2)
    };

    public static WorkOrderTestDataBuilder Create(TimeProvider provider) => new()
    {
        _provider = provider,
        _startAt = GetOpening(provider.GetUtcNow().UtcDateTime.AddDays(1)),
        _endAt = GetOpening(provider.GetUtcNow().UtcDateTime.AddDays(1)).AddHours(2)
    };

    public WorkOrderTestDataBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public WorkOrderTestDataBuilder WithVehicle(Guid id)
    {
        _vehicleId = id;
        return this;
    }

    public WorkOrderTestDataBuilder WithTimeSlot(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        _startAt = startAt;
        _endAt = endAt;
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(Guid id)
    {
        _laborId = id;
        return this;
    }

    public WorkOrderTestDataBuilder WithLabor(string id)
    {
        return WithLabor(Guid.Parse(id));
    }

    public WorkOrderTestDataBuilder AtSpot(Spot spot)
    {
        _spot = spot;
        return this;
    }

    public WorkOrderTestDataBuilder WithRepairTasks(params RepairTask[] repairTasks)
    {
        _repairTasks = repairTasks.ToList();
        return this;
    }
    public WorkOrderTestDataBuilder WithRepairTasks(List<RepairTask> repairTasks)
    {
        _repairTasks = repairTasks;
        return this;
    }

    public WorkOrderTestDataBuilder WithState(WorkOrderState state)
    {
        _state = state;
        return this;
    }

    public WorkOrderTestDataBuilder UpdateDate(TimeProvider provider, int hoursOffset = 0)
    {
        var date = provider?.GetUtcNow() ?? DateTimeOffset.UtcNow;
        var opening = GetOpening(date.UtcDateTime).UtcDateTime;
        _startAt = opening.AddHours(hoursOffset);
        _endAt = _startAt.AddHours(2);

        return this;
    }
    public WorkOrderTestDataBuilder UpdateDate(DateTime date, int hoursOffset = 0)
    {
        var opening = GetOpening(date).UtcDateTime;
        _startAt = opening.AddHours(hoursOffset);
        _endAt = _startAt.AddHours(2);

        return this;
    }

    public WorkOrderTestDataBuilder UpdateDate(int hoursOffset = 0)
    {
        _startAt = _startAt.AddHours(hoursOffset);
        _endAt = _startAt.AddHours(2);

        return this;
    }

    public WorkOrderTestDataBuilder InProgress()
    {
        _state = WorkOrderState.InProgress;
        return this;
    }

    public WorkOrderTestDataBuilder Completed()
    {
        _state = WorkOrderState.Completed;
        return this;
    }
    public WorkOrder Build()
    {
        var workOrder = WorkOrder.Create(
            id: _id,
            vehicleId: _vehicleId,
            startAt: _startAt,
            endAt: _endAt,
            laborId: _laborId,
            spot: _spot,
            repairTasks: _repairTasks).Value;

        if (_state == WorkOrderState.InProgress)
            workOrder.UpdateState(WorkOrderState.InProgress);

        if (_state == WorkOrderState.Completed)
        {
            workOrder.UpdateState(WorkOrderState.InProgress);
            workOrder.UpdateState(WorkOrderState.Completed);
        }

        if (_state == WorkOrderState.Cancelled)
            workOrder.UpdateState(WorkOrderState.Cancelled);

        return workOrder;
    }

    public static DateTimeOffset GetOpening(DateTime today)
    {
        var date = DateOnly.FromDateTime(today);

        return new DateTimeOffset(
        date.ToDateTime(AppSettingsTestData.DefaultOpeningTime),
        TimeSpan.Zero);
    }

}
