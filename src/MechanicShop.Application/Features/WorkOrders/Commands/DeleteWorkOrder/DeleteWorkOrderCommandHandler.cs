using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder
{
    public sealed class DeleteWorkOrderCommandHandler(ILogger<DeleteWorkOrderCommandHandler> logger, IAppDbContext context) : IRequestHandler<DeleteWorkOrderCommand, Result<Deleted>>
    {
        private readonly ILogger<DeleteWorkOrderCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<Deleted>> Handle(DeleteWorkOrderCommand command, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.FindAsync([command.WorkOrderId], ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", command.WorkOrderId);

                return ApplicationErrors.WorkOrderNotFound;
            }


            if (workOrder.State is not WorkOrderState.Scheduled)
            {
                _logger.LogError(
                    "Deletion failed: only 'Scheduled' WorkOrders can be deleted. Current status: {Status}",
                    workOrder.State);

                return WorkOrderErrors.Readonly;
            }

            _context.WorkOrders.Remove(workOrder);

            workOrder.AddDomainEvent(new WorkOrderCollectionModified());

            await _context.SaveChangesAsync(ct);

            return Result.Deleted;
        }
    }
}
