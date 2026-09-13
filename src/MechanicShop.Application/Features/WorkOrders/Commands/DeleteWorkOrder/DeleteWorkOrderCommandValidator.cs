using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder
{
    public class DeleteWorkOrderCommandValidator : AbstractValidator<DeleteWorkOrderCommand>
    {
        public DeleteWorkOrderCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
                .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);
        }
    }
}
