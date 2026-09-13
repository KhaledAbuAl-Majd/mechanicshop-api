using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Api.Requests.V1.WorkOrders
{
    public sealed record CreateWorkOrderRequest(
       Spot Spot,
       Guid VehicleId,
       DateTimeOffset StartAtUtc,
       List<Guid> RepairTaskIds,
       Guid LaborId);

}
