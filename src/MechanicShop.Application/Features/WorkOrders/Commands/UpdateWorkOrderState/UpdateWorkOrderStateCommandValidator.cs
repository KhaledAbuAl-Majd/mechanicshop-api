using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderState
{
    public sealed class UpdateWorkOrderStateCommandValidator : AbstractValidator<UpdateWorkOrderStateCommand>
    {
        public UpdateWorkOrderStateCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
              .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
              .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);

            RuleFor(x => x.State)
            .IsInEnum()
            .WithErrorCode("WorkOrderStatus.Invalid")
            .WithMessage("Status must be a valid WorkOrderStatus value.");
        }
    }
}
