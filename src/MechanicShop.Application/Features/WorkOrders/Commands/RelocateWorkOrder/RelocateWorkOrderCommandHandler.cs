using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public sealed class RelocateWorkOrderCommandHandler(
        ILogger<RelocateWorkOrderCommandHandler> logger,
        IAppDbContext context,
        IWorkOrderPolicy workOrderValidator) : IRequestHandler<RelocateWorkOrderCommand, Result<Updated>>
    {

        private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly IWorkOrderPolicy _workOrderPolicy = workOrderValidator;
        public async Task<Result<Updated>> Handle(RelocateWorkOrderCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders
                .FirstOrDefaultAsync(wo => wo.Id == command.WorkOrderId, ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }

            var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
            var endAt = command.NewStartAt.Add(duration);

            if (_workOrderPolicy.IsOutsideOperatingHours(command.NewStartAt, duration))
            {
                _logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", command.NewStartAt, endAt);

                return ApplicationErrors.WorkOrderOutsideOperatingHour(command.NewStartAt, endAt);
            }

            var checkMinRequirementResult = _workOrderPolicy.ValidateMinimumRequirement(command.NewStartAt, endAt);

            if (checkMinRequirementResult.IsError)
            {
                _logger.LogError("WorkOrder duration is shorter than the configured minimum.");

                return checkMinRequirementResult.Errors;
            }

            var checkSpotAvailabilityResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
                command.NewSpot,
                command.NewStartAt,
                endAt,
                excludeWorkOrderId: workOrder.Id,
                ct);

            if (checkSpotAvailabilityResult.IsError)
            {
                _logger.LogError("Spot: {Spot} is not available.", workOrder.Spot.ToString());
                return checkSpotAvailabilityResult.Errors;
            }


            var CheckVehicleConflictResult = await _workOrderPolicy.IsVehicleAlreadyScheduled(
                workOrder.VehicleId,
                command.NewStartAt,
                endAt,
                excludedWorkOrderId: workOrder.Id,
                ct);

            if (CheckVehicleConflictResult.IsError)
            {
                _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", workOrder.VehicleId);

                return CheckVehicleConflictResult.Errors;
            }

            var CheckLaborOccupiedResult = await _workOrderPolicy.IsLaborOccupied(workOrder.LaborId, workOrder.Id, command.NewStartAt, endAt, ct);

            if (CheckLaborOccupiedResult.IsError)
            {
                _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", workOrder.LaborId);
                return CheckLaborOccupiedResult.Errors;
            }

            var updateTimingResult = workOrder.UpdateTiming(command.NewStartAt, endAt);

            if (updateTimingResult.IsError)
            {
                _logger.LogError("Failed to update timing: {Error}", updateTimingResult.TopError.Description);

                return updateTimingResult.Errors;
            }

            var updateSpotResult = workOrder.UpdateSpot(command.NewSpot);

            if (updateSpotResult.IsError)
            {
                _logger.LogError("Failed to update Spot: {Error}", updateSpotResult.TopError.Description);

                return updateSpotResult.Errors;
            }

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' reallocated successfully.", workOrder.Id);

            return Result.Updated;
        }
    }
}
