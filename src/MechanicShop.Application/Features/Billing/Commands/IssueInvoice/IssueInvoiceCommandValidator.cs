using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.Billing.Commands.IssueInvoice
{
    public sealed class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
    {
        public IssueInvoiceCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
                .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);
        }
    }
}
