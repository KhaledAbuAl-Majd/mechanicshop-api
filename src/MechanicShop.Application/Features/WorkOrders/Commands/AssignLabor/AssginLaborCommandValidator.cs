using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor
{
    public class AssginLaborCommandValidator : AbstractValidator<AssignLaborCommand>
    {
        public AssginLaborCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
                .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);

            RuleFor(x => x.LaborId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.LaborIdRequired.Code)
                .WithMessage(WorkOrderErrors.LaborIdRequired.Description);
        }
    }
}
