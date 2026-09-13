using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Constants;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(string UserId) : ICachedQuery<Result<AppUserDto>>
    {
        public string CacheKey => UserCache.ByIdKey(UserId);

        public string[] Tags => [UserCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(20);
    }
}
