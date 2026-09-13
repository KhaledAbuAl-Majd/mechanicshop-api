using FluentValidation;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks
{
    public sealed class UpdateWorkOrderRepairTasksCommandValidator : AbstractValidator<UpdateWorkOrderRepairTasksCommand>
    {
        public UpdateWorkOrderRepairTasksCommandValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
            .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
            .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);

            RuleFor(x => x.RepairTaskIds).NotEmpty()
                .WithErrorCode(RepairTaskErrors.AtLeastOneRepairTaskIsRequired.Code)
                .WithMessage(RepairTaskErrors.AtLeastOneRepairTaskIsRequired.Description);
        }
    }
}
