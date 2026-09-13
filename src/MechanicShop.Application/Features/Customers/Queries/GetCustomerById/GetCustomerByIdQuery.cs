using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed record GetCustomerByIdQuery(Guid CustomerId) : ICachedQuery<Result<CustomerDto>>
    {
        public string CacheKey => CustomerCache.ByIdKey(CustomerId);

        public TimeSpan Expiration => TimeSpan.FromMinutes(10);

        public string[] Tags => [CustomerCache.Tag];
    }

}
