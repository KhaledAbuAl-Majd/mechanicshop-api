using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.WorkOrders.Billing.InvoiceLineItems
{
    public static class InvoiceLineItemErrors
    {
        public static readonly Error InvoiceIdRequired = Error.Validation(
    code: "InvoiceLineItemErrors.InvoiceIdRequired",
    description: "InvoiceId is required.");

        public static readonly Error LineNumberInvalid = Error.Validation(
            code: "InvoiceLineItemErrors.LineNumberInvalid",
            description: "Line number must be greater than 0.");

        public static readonly Error DescriptionRequired = Error.Validation(
            code: "InvoiceLineItemErrors.DescriptionRequired",
            description: "Description is required.");

        public static readonly Error QuantityInvalid = Error.Validation(
            code: "InvoiceLineItemErrors.QuantityInvalid",
            description: "Quantity must be greater than 0.");

        public static readonly Error UnitPriceInvalid = Error.Validation(
            code: "InvoiceLineItemErrors.UnitPriceInvalid",
            description: "Unit price must be greater than 0.");
    }
}
