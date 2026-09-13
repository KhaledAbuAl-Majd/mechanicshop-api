using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats
{
    public sealed record GetWorkOrderStatsQuery(
        TimeZoneInfo TimeZone,
        DateOnly Date) : ICachedQuery<Result<TodayWorkOrderStatsDto>>
    {
        public string CacheKey => $"dashboard:stats:{Date:yyyy-MM-dd}:tz={TimeZone.Id}";

        public string[] Tags => [WorkOrderCache.Tag, InvoiceCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
