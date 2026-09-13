using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState
{
    public sealed record UpdateWorkOrderStateCommand(
        Guid WorkOrderId,
        WorkOrderState State) : IInvalidateCacheCommand<Result<Updated>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
