using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks
{
    public sealed record class GetRepairTasksQuery : ICachedQuery<Result<List<RepairTaskDto>>>
    {
        public string CacheKey => RepairTaskCache.AllKey;

        public string[] Tags => [RepairTaskCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
