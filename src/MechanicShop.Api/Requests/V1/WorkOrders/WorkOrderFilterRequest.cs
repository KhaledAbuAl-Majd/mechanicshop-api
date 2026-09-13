using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Api.Requests.V1.WorkOrders
{
    public sealed record WorkOrderFilterRequest
    (
        string? SearchTerm,
        string SortColumn = "createdAt",
        string SortDirection = "desc",
        WorkOrderState? State = null,
        Guid? VehicleId = null,
        Guid? LaborId = null,
        DateTime? StartDateFrom = null,
        DateTime? StartDateTo = null,
        DateTime? EndDateFrom = null,
        DateTime? EndDateTo = null,
        Spot? Spot = null);

}
