using MechanicShop.Application.Common;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Infrastructure.Services
{
    public class WorkOrderNotifier(ILogger<WorkOrderNotifier> logger) : IWorkOrderNotifier
    {
        public Task NotifyWorkOrdersChangedAsync(CancellationToken ct = default)
        {
            logger.LogInformation("Work order changed");

            return Task.CompletedTask;
        }
    }
}
