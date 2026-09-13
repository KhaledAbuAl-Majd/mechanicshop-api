using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.WorkOrders.EventHandlers
{
    public sealed class SendWorkOrderCompletedEmailHandler(
        INotificationService notificationService,
        IAppDbContext context,
        ILogger<SendWorkOrderCompletedEmailHandler> logger) : INotificationHandler<WorkOrderCompleted>
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly IAppDbContext _context = context;
        private readonly ILogger<SendWorkOrderCompletedEmailHandler> _logger = logger;
        public async Task Handle(WorkOrderCompleted notification, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.AsNoTracking()
                .Include(wo => wo.Vehicle!)
                .ThenInclude(v => v.Customer)
                .FirstOrDefaultAsync(wo => wo.Id == notification.WorkOrderId, ct);

            if (workOrder is null)
            {
                _logger.LogError("WorkOrder with Id '{WorkOrderId}' does not exist.", notification.WorkOrderId);
                return;
            }

            var customer = workOrder.Vehicle?.Customer;

            if (!string.IsNullOrWhiteSpace(customer?.Email))
            {
                await _notificationService.SendEmailAsync(customer.Email, ct);
            }

            if (!string.IsNullOrWhiteSpace(customer?.PhoneNumber))
            {
                await _notificationService.SendSmsAsync(customer.PhoneNumber, ct);
            }
        }
    }
}
