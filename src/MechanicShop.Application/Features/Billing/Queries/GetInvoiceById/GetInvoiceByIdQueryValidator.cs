using FluentValidation;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Queries.GetInvoiceById
{
    public sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
    {
        public GetInvoiceByIdQueryValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty()
              .WithErrorCode(InvoiceErrors.IdRequired.Code)
              .WithMessage(InvoiceErrors.IdRequired.Description);
        }
    }
}
