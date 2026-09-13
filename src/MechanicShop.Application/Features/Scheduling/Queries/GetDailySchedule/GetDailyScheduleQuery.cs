using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.WorkOrders.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Scheduling.Queries.GetDailySchedule
{
    public sealed record GetDailyScheduleQuery(
     TimeZoneInfo TimeZone,
     DateOnly ScheduleDate,
     Guid? LaborId = null) : ICachedQuery<Result<ScheduleDto>>
    {
        public string CacheKey =>
            $"{WorkOrderCache.AllKey}:daily:{ScheduleDate:yyyy-MM-dd}:labor={LaborId?.ToString() ?? "-"}:tz={TimeZone.Id}";

        public string[] Tags => [WorkOrderCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
