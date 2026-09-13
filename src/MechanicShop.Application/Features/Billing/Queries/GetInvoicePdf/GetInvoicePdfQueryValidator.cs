using FluentValidation;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf
{
    public sealed class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
    {
        public GetInvoicePdfQueryValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty()
              .WithErrorCode(InvoiceErrors.IdRequired.Code)
              .WithMessage(InvoiceErrors.IdRequired.Description);
        }
    }
}
