using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState
{
    public sealed class UpdateWorkOrderStateCommandHandler(
        ILogger<UpdateWorkOrderStateCommandHandler> logger,
        IAppDbContext context,
        TimeProvider dateTime) : IRequestHandler<UpdateWorkOrderStateCommand, Result<Updated>>
    {
        private readonly ILogger<UpdateWorkOrderStateCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        private readonly TimeProvider _dateTime = dateTime;
        public async Task<Result<Updated>> Handle(UpdateWorkOrderStateCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.FindAsync([command.WorkOrderId], ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }

            if (workOrder.StartAtUtc > _dateTime.GetUtcNow())
            {
                _logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work order�s scheduled start time.", command.WorkOrderId);

                return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc);
            }

            var updateStatusResult = workOrder.UpdateState(command.State);

            if (updateStatusResult.IsError)
            {
                _logger.LogError("Failed to update status: {Error}", updateStatusResult.TopError.Description);
                return updateStatusResult.Errors;
            }

            if (workOrder.State == WorkOrderState.Completed)
            {
                workOrder.AddDomainEvent(new WorkOrderCompleted() { WorkOrderId = workOrder.Id });
            }

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("WorkOrder with Id '{WorkOrderId}' updated stats successfully.", workOrder.Id);

            return Result.Updated;
        }
    }
}
