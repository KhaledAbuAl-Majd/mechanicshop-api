using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Constants;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabors
{
    public sealed record GetLaborsQuery : ICachedQuery<Result<List<LaborDto>>>
    {
        public string CacheKey => LaborCache.AllKey;

        public string[] Tags => [LaborCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
