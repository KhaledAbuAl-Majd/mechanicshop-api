using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice
{
    public sealed record SettleInvoiceCommand(Guid InvoiceId) : IInvalidateCacheCommand<Result<Success>>
    {
        public string[] Tags => [InvoiceCache.Tag];
    }
}
