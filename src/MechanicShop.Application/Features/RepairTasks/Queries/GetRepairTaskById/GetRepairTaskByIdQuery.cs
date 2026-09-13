using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Constants;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById
{
    public sealed record GetRepairTaskByIdQuery(Guid RepairTaskId) : ICachedQuery<Result<RepairTaskDto>>
    {
        public string CacheKey => RepairTaskCache.ByIdKey(RepairTaskId);

        public string[] Tags => [RepairTaskCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
