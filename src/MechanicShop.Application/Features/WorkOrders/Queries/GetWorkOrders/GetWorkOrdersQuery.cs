using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Enums;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders
{
    public sealed record GetWorkOrdersQuery(int Page,
        int PageSize,
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
        Spot? Spot = null) : ICachedQuery<Result<PaginatedList<WorkOrderListItemDto>>>
    {
        public string CacheKey =>
            $"{WorkOrderCache.AllKey}:p={Page}:ps={PageSize}" +
            $":q={SearchTerm ?? "-"}" +
            $":sort={SortColumn}:{SortDirection}" +
            $":state={State?.ToString() ?? "-"}" +
            $":veh={VehicleId?.ToString() ?? "-"}" +
            $":lab={LaborId?.ToString() ?? "-"}" +
            $":sdfrom={StartDateFrom?.ToString() ?? "-"}" +
            $":sdto={StartDateTo?.ToString() ?? "-"}" +
            $":edfrom={EndDateFrom?.ToString() ?? "-"}" +
            $":edto={EndDateTo?.ToString() ?? "-"}" +
            $":spot={Spot?.ToString() ?? "-"}";

        public string[] Tags => [WorkOrderCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
