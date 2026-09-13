using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById
{
    public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : ICachedQuery<Result<InvoiceDto>>
    {
        public string CacheKey => InvoiceCache.ByIdKey(InvoiceId);

        public string[] Tags => [InvoiceCache.Tag];

        public TimeSpan Expiration => TimeSpan.FromMinutes(30);
    }
}
