using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks
{
    public sealed record UpdateWorkOrderRepairTasksCommand(
        Guid WorkOrderId,
        Guid[] RepairTaskIds) : IInvalidateCacheCommand<Result<Updated>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
