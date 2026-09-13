using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public sealed class RelocateWorkOrderCommandValidator : AbstractValidator<RelocateWorkOrderCommand>
    {
        public RelocateWorkOrderCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
              .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
              .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);

            RuleFor(x => x.NewStartAt).GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("New StartAt must be in the future.");

            RuleFor(x => x.NewSpot).IsInEnum()
                .WithErrorCode(WorkOrderErrors.SpotInvalid.Code)
                .WithMessage("Spot must be a valid Spot value. [A, B, C, D]");
        }
    }
}
