using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder
{
    public sealed record CreateWorkOrderCommand(
        Spot Spot,
        Guid VehicleId,
        DateTimeOffset StartAt,
        List<Guid> RepairTaskIds,
        Guid LaborId) : IInvalidateCacheCommand<Result<WorkOrderDto>>
    {
        public string[] Tags => [WorkOrderCache.Tag];
    }
}
