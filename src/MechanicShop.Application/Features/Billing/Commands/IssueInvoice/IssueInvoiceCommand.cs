using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Constants;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public sealed record IssueInvoiceCommand(Guid WorkOrderId) : IInvalidateCacheCommand<Result<InvoiceDto>>
    {
        public string[] Tags => [InvoiceCache.Tag];
    }
}
