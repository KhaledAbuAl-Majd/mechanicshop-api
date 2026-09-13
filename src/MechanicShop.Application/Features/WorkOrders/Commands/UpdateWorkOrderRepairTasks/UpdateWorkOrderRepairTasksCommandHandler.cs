using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks
{
    public sealed class UpdateWorkOrderRepairTasksCommandHandler(
        ILogger<UpdateWorkOrderRepairTasksCommandHandler> logger,
        IAppDbContext context,
        IWorkOrderPolicy workOrderValidator) : IRequestHandler<UpdateWorkOrderRepairTasksCommand, Result<Updated>>
    {
        private readonly ILogger<UpdateWorkOrderRepairTasksCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly IWorkOrderPolicy _workOrderValidator = workOrderValidator;

        public async Task<Result<Updated>> Handle(UpdateWorkOrderRepairTasksCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.Include(wo => wo.RepairTasks).FirstOrDefaultAsync(wo => wo.Id == command.WorkOrderId, ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }

            if (command.RepairTaskIds.Length == 0)
            {
                _logger.LogError("Empty RepairTaskIds list submitted.");

                return RepairTaskErrors.AtLeastOneRepairTaskIsRequired;
            }

            var requestRepairTask = await _context.RepairTasks.Where(rt => command.RepairTaskIds.Contains(rt.Id)).ToListAsync(ct);

            if (requestRepairTask.Count != command.RepairTaskIds.Length)
            {
                var missingIds = command.RepairTaskIds.Except(requestRepairTask.Select(rt => rt.Id));

                _logger.LogError("One or more RepairTasks not found. {ids}", string.Join(", ", missingIds));

                return ApplicationErrors.RepairTaskNotFound;
            }

            var clearExistingResult = workOrder.ClearRepairTasks();

            if (clearExistingResult.IsError)
                return clearExistingResult.Errors;

            foreach (var task in requestRepairTask)
            {
                var addRepairTaskResult = workOrder.AddRepairtTask(task);

                if (addRepairTaskResult.IsError)
                    return addRepairTaskResult.Errors;
            }

            var totalDuration = TimeSpan.FromMinutes(workOrder.RepairTasks.Sum(rt => (int)rt.EstimatedDurationInMins));
            var newEndAt = workOrder.StartAtUtc.Add(totalDuration);

            if (_workOrderValidator.IsOutsideOperatingHours(workOrder.StartAtUtc, totalDuration))
            {
                return ApplicationErrors.WorkOrderOutsideOperatingHour(workOrder.StartAtUtc, newEndAt);
            }

            var spotCheckResult = await _workOrderValidator.CheckSpotAvailabilityAsync(
                workOrder.Spot,
                workOrder.StartAtUtc,
                newEndAt,
                excludeWorkOrderId: workOrder.Id,
                ct: ct);

            if (spotCheckResult.IsError)
            {
                return spotCheckResult.Errors;
            }

            var checkLaborOccupiedResult = await _workOrderValidator.IsLaborOccupied(workOrder.LaborId, workOrder.Id, workOrder.StartAtUtc, newEndAt);

            if (checkLaborOccupiedResult.IsError)
            {
                return checkLaborOccupiedResult.Errors;
            }

            var updateTimingResult = workOrder.UpdateTiming(workOrder.StartAtUtc, newEndAt);

            if (updateTimingResult.IsError)
                return updateTimingResult.Errors;

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' updated repair tasks successfully.", workOrder.Id);

            return Result.Updated;
        }
    }
}
