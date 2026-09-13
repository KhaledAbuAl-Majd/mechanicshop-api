using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public sealed record RelocateWorkOrderCommand(
      Guid WorkOrderId,
      DateTimeOffset NewStartAt,
      Spot NewSpot) : IInvalidateCacheCommand<Result<Updated>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
