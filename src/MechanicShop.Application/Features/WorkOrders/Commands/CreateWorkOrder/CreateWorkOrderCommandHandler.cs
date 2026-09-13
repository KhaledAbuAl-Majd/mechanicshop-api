using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder
{
    public sealed class CreateWorkOrderCommandHandler(
        ILogger<CreateWorkOrderCommandHandler> logger,
        IAppDbContext context,
        IWorkOrderPolicy workOrderValidator) : IRequestHandler<CreateWorkOrderCommand, Result<WorkOrderDto>>
    {
        private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly IWorkOrderPolicy _workOrderPolicy = workOrderValidator;
        public async Task<Result<WorkOrderDto>> Handle(CreateWorkOrderCommand command, CancellationToken ct)
        {
            var repairTasks = await _context.RepairTasks.Where(rt => command.RepairTaskIds.Contains(rt.Id)).ToListAsync(ct);

            if (repairTasks.Count != command.RepairTaskIds.Count)
            {
                var missingIds = command.RepairTaskIds.Except(repairTasks.Select(rt => rt.Id)).ToArray();

                _logger.LogWarning("Some RepairTaskIds not found: {MissingIds}", string.Join(", ", missingIds));

                return ApplicationErrors.RepairTaskNotFound;
            }

            var totalEstimatedDuration = TimeSpan.FromMinutes(repairTasks.Sum(rt => (int)rt.EstimatedDurationInMins));
            var endAt = command.StartAt.Add(totalEstimatedDuration);

            if (_workOrderPolicy.IsOutsideOperatingHours(command.StartAt, totalEstimatedDuration))
            {
                _logger.LogError("The WorkOrder time ({StartAt} ? {EndAt}) is outside of store operating hours.", command.StartAt, endAt);

                return ApplicationErrors.WorkOrderOutsideOperatingHour(command.StartAt, endAt);
            }

            var checkMinRequirementResult = _workOrderPolicy.ValidateMinimumRequirement(command.StartAt, endAt);

            if (checkMinRequirementResult.IsError)
            {
                _logger.LogError("WorkOrder duration is shorter than the configured minimum.");

                return checkMinRequirementResult.Errors;
            }

            var checkSpotAvailabilityResult = await _workOrderPolicy.CheckSpotAvailabilityAsync(
                command.Spot,
                command.StartAt,
                endAt,
                excludeWorkOrderId: null,
                ct);

            if (checkSpotAvailabilityResult.IsError)
            {
                _logger.LogError("Spot: {Spot} is not available.", command.Spot.ToString());
                return checkSpotAvailabilityResult.Errors;
            }

            var vehicle = await _context.Vehicles.Include(v => v.Customer).FirstOrDefaultAsync(v => v.Id == command.VehicleId, ct);

            if (vehicle is null)
            {
                _logger.LogError("Vehicle with Id '{VehicleId}' does not exist.", command.VehicleId);

                return ApplicationErrors.VehicleNotFound;
            }

            var labor = await _context.Employees.FindAsync([command.LaborId], ct);

            if (labor is null)
            {
                _logger.LogError("Invalid LaborId: {LaborId}", command.LaborId.ToString());
                return ApplicationErrors.LaborNotFound;
            }

            var CheckVehicleConflictResult = await _workOrderPolicy.IsVehicleAlreadyScheduled(
                command.VehicleId,
                command.StartAt,
                endAt,
                excludedWorkOrderId: null,
                ct);

            if (CheckVehicleConflictResult.IsError)
            {
                _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", command.VehicleId);

                return CheckVehicleConflictResult.Errors;
            }

            var CheckLaborOccupiedResult = await _workOrderPolicy.IsLaborOccupied(command.LaborId, null, command.StartAt, endAt, ct);



            if (CheckLaborOccupiedResult.IsError)
            {
                _logger.LogError("Labor with Id '{LaborId}' is already occupied during the requested time.", command.LaborId);
                return CheckLaborOccupiedResult.Errors;
            }

            var createWorkOrderResult = WorkOrder.Create(
                Guid.NewGuid(),
                command.VehicleId,
                command.StartAt,
                endAt,
                command.LaborId,
                command.Spot,
                repairTasks);
            if (createWorkOrderResult.IsError)
            {
                _logger.LogError("Failed to create WorkOrder: {Error}", createWorkOrderResult.TopError.Description);

                return createWorkOrderResult.Errors;
            }

            var workOrder = createWorkOrderResult.Value;

            _context.WorkOrders.Add(workOrder);

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await _context.SaveChangesAsync(ct);

            workOrder.Vehicle = vehicle;
            workOrder.Labor = labor;

            _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' created successfully.", workOrder.Id);

            return workOrder.ToDto();
        }

    }
}
