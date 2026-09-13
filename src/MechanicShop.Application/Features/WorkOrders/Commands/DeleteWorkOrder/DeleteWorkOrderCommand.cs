using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder
{
    public sealed record DeleteWorkOrderCommand(Guid WorkOrderId) : IInvalidateCacheCommand<Result<Deleted>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
