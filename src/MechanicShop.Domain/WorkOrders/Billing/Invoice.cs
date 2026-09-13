using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.WorkOrders.Billing.Enums;
using MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems;

namespace MechanicShop.Domain.WorkOrders.Billing
{
    public sealed class Invoice : AuditableEntity
    {
        public Guid WorkOrderId { get; }
        public DateTimeOffset IssuedAtUtc { get; }
        public decimal DiscountAmount { get; private set; }
        public decimal TaxAmount { get; }
        public decimal SubTotal => _lineItems.Sum(x => x.LineTotal);
        public decimal Total => SubTotal - DiscountAmount + TaxAmount;

        public DateTimeOffset? PaidAt { get; private set; }
        public WorkOrder? WorkOrder { get; set; }

        private readonly List<InvoiceLineItem> _lineItems = [];
        public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
        public InvoiceStatus Status { get; private set; }

        private Invoice()
        { }

        private Invoice(Guid id, Guid workOrderId, DateTimeOffset issuedAt, List<InvoiceLineItem> lineItems, decimal discountAmount, decimal taxAmount)
            : base(id)
        {
            WorkOrderId = workOrderId;
            IssuedAtUtc = issuedAt;
            DiscountAmount = discountAmount;
            Status = InvoiceStatus.Unpaid;
            TaxAmount = taxAmount;
            _lineItems = lineItems ?? [];
        }

        public static Result<Invoice> Create(Guid id, Guid workOrderId, List<InvoiceLineItem> items, decimal discountAmount, decimal taxAmount, TimeProvider datetime)
        {
            if (id == Guid.Empty)
                return InvoiceErrors.IdRequired;

            if (workOrderId == Guid.Empty)
                return InvoiceErrors.WorkOrderIdInvalid;

            if (items is null || items.Count == 0)
                return InvoiceErrors.LineItemsEmpty;

            if (discountAmount < 0)
                return InvoiceErrors.DiscountNegative;

            var subTotal = items.Sum(x => x.LineTotal);

            if (discountAmount > subTotal)
                return InvoiceErrors.DiscountExceedsSubtotal;

            if (taxAmount < 0)
                return InvoiceErrors.TaxNegative;

            return new Invoice(id, workOrderId, datetime.GetUtcNow(), items, discountAmount, taxAmount);
        }

        public Result<Updated> ApplyDiscount(decimal discountAmount)
        {
            if (Status is not InvoiceStatus.Unpaid)
                return InvoiceErrors.InvoiceLocked;

            if (discountAmount < 0)
                return InvoiceErrors.DiscountNegative;

            if (discountAmount > SubTotal)
                return InvoiceErrors.DiscountExceedsSubtotal;

            DiscountAmount = discountAmount;

            return Result.Updated;
        }

        public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
        {
            if (Status is not InvoiceStatus.Unpaid)
                return InvoiceErrors.InvoiceLocked;

            Status = InvoiceStatus.Paid;
            PaidAt = timeProvider.GetUtcNow();

            return Result.Updated;
        }
    }
}
