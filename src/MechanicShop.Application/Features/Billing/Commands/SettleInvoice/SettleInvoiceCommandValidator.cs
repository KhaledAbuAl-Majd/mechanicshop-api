using FluentValidation;
using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice
{
    public sealed class SettleInvoiceCommandValidator : AbstractValidator<SettleInvoiceCommand>
    {
        public SettleInvoiceCommandValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty()
               .WithErrorCode(InvoiceErrors.IdRequired.Code)
               .WithMessage(InvoiceErrors.IdRequired.Description);
        }
    }
}
