using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems;

namespace MechanicShop.Tests.Common.WorkOrders.Billing;

public static class InvoiceFactory
{
    public static Result<Invoice> CreateInvoice(
        Guid? id = null,
        Guid? workOrderId = null,
        List<InvoiceLineItem>? items = null,
        decimal? discountAmount = null,
        decimal? taxAmount = null,
        TimeProvider? datetime = null, bool setListIfNull = true)
    {
        return Invoice.Create(
            id ?? Guid.NewGuid(),
            workOrderId ?? Guid.NewGuid(),
           setListIfNull ? items ?? [InvoiceLineItemFactory.CreateInvoiceLineItem().Value] : items!,
            discountAmount ?? 0,
            taxAmount ?? 0,
            datetime ?? TimeProvider.System);
    }
}