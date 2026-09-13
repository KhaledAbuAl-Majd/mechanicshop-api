using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Api.Requests.V1.WorkOrders
{
    public sealed record RelocateWorkOrderRequest(
     DateTimeOffset NewStartAtUtc,
     Spot NewSpot);

}
