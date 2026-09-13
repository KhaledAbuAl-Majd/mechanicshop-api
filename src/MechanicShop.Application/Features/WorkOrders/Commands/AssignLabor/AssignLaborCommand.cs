using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor
{
    public sealed record AssignLaborCommand(Guid WorkOrderId, Guid LaborId) : IInvalidateCacheCommand<Result<Updated>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
