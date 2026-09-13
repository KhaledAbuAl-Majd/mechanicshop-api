using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Domain.WorkOrders
{
    public static class WorkOrderErrors
    {
        public static readonly Error WorkOrderIdRequired = Error.Validation(
      code: "WorkOrderErrors.WorkOrderIdRequired",
      description: "WorkOrder Id is required");

        public static readonly Error VehicleIdRequired = Error.Validation(
            code: "WorkOrderErrors.VehicleIdRequired",
            description: "Vehicle Id is required");

        public static readonly Error RepairTasksRequired = Error.Validation(
            code: "WorkOrderErrors.RepairTasksRequired",
            description: "At least one repair task is required");

        public static readonly Error RepairTaskInvalid = Error.Validation(
            code: "WorkOrderErrors.RepairTask.Invalid",
            description: "Invalid Repair Task");

        public static readonly Error LaborIdRequired = Error.Validation(
            code: "WorkOrderErrors.LaborIdRequired",
            description: "Labor Id is required");

        public static readonly Error InvalidTiming = Error.Conflict(
      code: "WorkOrderErrors.InvalidTiming",
      description: "End time must be after start time.");

        public static readonly Error SpotInvalid = Error.Validation(
            code: "WorkOrderErrors.SpotInvalid",
            description: "The provided spot is invalid");

        public static readonly Error Readonly = Error.Conflict(
            code: "WorkOrderErrors.Readonly",
            description: "WorkOrder is read-only.");

        public static Error TimingReadonly(string id, WorkOrderState state) => Error.Conflict(
      code: "WorkOrderErrors.TimingReadonly",
      description: $"WorkOrder '{id}': Can't Modify timing when WorkOrder status is '{state}'.");

        public static Error LaborIdEmpty(string id) => Error.Validation(
       code: "WorkOrderErrors.LaborIdEmpty",
       description: $"WorkOrder '{id}': Labor Id is empty");

        public static Error StateTransitionNotAllowed(DateTimeOffset startAtUtc) => Error.Conflict(
           code: "WorkOrderErrors.StateTransitionNotAllowed",
           description: $"State transition is not allowed before the work order’s scheduled start time {startAtUtc:yyyy-MM-dd HH:mm} UTC.");

        public static Error InvalidStateTransition(WorkOrderState current, WorkOrderState next) => Error.Conflict(
            code: "WorkOrderErrors.InvalidStateTransition",
            description: $"WorkOrder Invalid State transition from '{current}' to '{next}'.");

        public static readonly Error RepairTaskAlreadyAdded = Error.Conflict(
            code: "WorkOrderErrors.RepairTaskAlreadyAdded",
            description: "Repair task already exists.");
    }
}
