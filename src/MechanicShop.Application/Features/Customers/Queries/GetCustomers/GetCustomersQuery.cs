using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;


namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers
{
    public sealed record GetCustomersQuery(int Page, int PageSize) : ICachedQuery<Result<PaginatedList<CustomerListItemDto>>>
    {
        public string CacheKey => $"{CustomerCache.AllKey}:p={Page}:ps={PageSize}";

        public string[] Tags => [CustomerCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    }
}
