using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Interfaces
{
    public interface IInvoicePdfGenerator
    {
        byte[] Generate(Invoice invoice);
    }
}
