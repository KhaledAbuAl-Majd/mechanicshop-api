using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;

namespace MechanicShop.Domain.UnitTests.WorkOrders
{
    public class WorkOrderTests
    {
        [Fact]
        public void Create_ShouldReturnError_WhenIdIsEmpty()
        {
            var woResult = WorkOrderFactory.CreateWorkOrder(id: Guid.Empty);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, woResult.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenVehicleIdEmpty()
        {
            var woResult = WorkOrderFactory.CreateWorkOrder(vehicleId: Guid.Empty);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, woResult.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenNoRepairTasks()
        {
            var woResult = WorkOrderFactory.CreateWorkOrder(repairTasks: []);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, woResult.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenLaborIdEmpty()
        {
            var woResult = WorkOrderFactory.CreateWorkOrder(laborId: Guid.Empty);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, woResult.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenTiminInvalid()
        {
            var woResult = WorkOrderFactory.CreateWorkOrder(startAt: DateTimeOffset.UtcNow.AddHours(1), endAt: DateTimeOffset.UtcNow);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.InvalidTiming.Code, woResult.TopError.Code);
        }

        [Fact]
        public void Create_ShouldReturnError_WhenSpotInvalid()
        {
            var invalidSpot = (Spot)99999;
            var woResult = WorkOrderFactory.CreateWorkOrder(spot: invalidSpot);

            Assert.False(woResult.IsSuccess);
            Assert.Equal(WorkOrderErrors.SpotInvalid.Code, woResult.TopError.Code);
        }

        [Fact]
        public void AddRepairTask_ShouldReturnError_WhenRepairTaskIsNull()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var result = wo.AddRepairtTask(null!);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.RepairTaskInvalid.Code, result.TopError.Code);
        }

        [Fact]
        public void AddRepairTask_ShouldReturnError_WhenNotEditable()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var result = wo.AddRepairtTask(RepairTaskFactory.CreateRepairTask().Value);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
        }

        [Fact]
        public void AddRepairTask_ShouldReturnError_WhenRepairTaskAlreadyAdded()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var rt1 = RepairTaskFactory.CreateRepairTask().Value;
            var rt2 = RepairTaskFactory.CreateRepairTask(id: rt1.Id).Value;

            wo.AddRepairtTask(rt1);

            var result = wo.AddRepairtTask(rt2);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.RepairTaskAlreadyAdded.Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateTiming_ShouldReturnError_WhenNotEditable()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var startAt = DateTimeOffset.UtcNow;
            var endAt = DateTimeOffset.UtcNow.AddHours(1);

            var result = wo.UpdateTiming(startAt, endAt);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.TimingReadonly(wo.Id.ToString(), wo.State).Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateTiming_ShouldReturnError_WhenTimingInvalid()
        {
            var startAt = DateTimeOffset.UtcNow.AddHours(1);
            var endAt = DateTimeOffset.UtcNow;

            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var result = wo.UpdateTiming(startAt, endAt);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateTiming_ShouldReturnSuccess_AndSetNewTiming()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var startAt = DateTimeOffset.UtcNow;
            var endAt = DateTimeOffset.UtcNow.AddHours(1);

            var result = wo.UpdateTiming(startAt, endAt);

            Assert.True(result.IsSuccess);
            Assert.Equal(wo.StartAtUtc, startAt);
            Assert.Equal(wo.EndAtUtc, endAt);
        }

        [Fact]
        public void UpdateLabor_ShouldReturnError_WhenNotEditable()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var result = wo.UpdateLabor(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateLabor_ShouldReturnError_WhenLaborIdEmpty()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var result = wo.UpdateLabor(Guid.Empty);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.LaborIdEmpty(wo.Id.ToString()).Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateLabor_ShouldReturnSuccess_AndSetNewLabor()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var laborId = Guid.NewGuid();

            var result = wo.UpdateLabor(laborId);

            Assert.True(result.IsSuccess);
            Assert.Equal(wo.LaborId, laborId);
        }

        [Fact]
        public void UpdateState_ShouldReturnError_WhenTransitionInvalidFromScheduledToCompleted()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var newState = WorkOrderState.Completed;

            var result = wo.UpdateState(newState);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.InvalidStateTransition(wo.State, newState).Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateState_ShouldReturnError_WhenTransitionInvalidFromCompletedToCancel()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var newState = WorkOrderState.Cancelled;

            var result = wo.UpdateState(newState);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.InvalidStateTransition(wo.State, newState).Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateState_ShouldReturnSuccess_AndSetStateToInProgress()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var newState = WorkOrderState.InProgress;

            var result = wo.UpdateState(newState);

            Assert.True(result.IsSuccess);
            Assert.Equal(wo.State, newState);
        }

        [Fact]
        public void UpdateState_ShouldReturnSuccess_AndSetStateToCompleted()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);

            var newState = WorkOrderState.Completed;
            var result = wo.UpdateState(newState);

            Assert.True(result.IsSuccess);
            Assert.Equal(wo.State, newState);
        }

        [Theory]
        [InlineData(WorkOrderState.Scheduled)]
        [InlineData(WorkOrderState.InProgress)]
        public void UpdateState_ShouldReturnSuccess_WhenCancelledFromValidStates(WorkOrderState initialState)
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            if (initialState == WorkOrderState.InProgress)
            {
                wo.UpdateState(WorkOrderState.InProgress);
            }

            var result = wo.UpdateState(WorkOrderState.Cancelled);

            Assert.True(result.IsSuccess);
            Assert.Equal(WorkOrderState.Cancelled, wo.State);
        }

        [Fact]
        public void UpdateSpot_ShouldReturnError_WhenNotEditable()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var result = wo.UpdateSpot(Spot.D);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
        }

        [Fact]
        public void ClearRepairTasks_ShouldReturnError_WhenNotEditable()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            wo.UpdateState(WorkOrderState.InProgress);
            wo.UpdateState(WorkOrderState.Completed);

            var result = wo.ClearRepairTasks();

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.Readonly.Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateSpot_ShouldReturnError_SpotInvalid()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var newSpot = (Spot)9999;

            var result = wo.UpdateSpot(newSpot);

            Assert.False(result.IsSuccess);
            Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
        }

        [Fact]
        public void UpdateSpot_ShouldReturnSuccess_AndSetNewSpot()
        {
            var wo = WorkOrderFactory.CreateWorkOrder().Value;

            var newSpot = Spot.D;

            var result = wo.UpdateSpot(newSpot);

            Assert.True(result.IsSuccess);
            Assert.Equal(wo.Spot, newSpot);
        }
    }
}
