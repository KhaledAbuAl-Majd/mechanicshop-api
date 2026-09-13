using FluentValidation;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask
{
    public class RemoveRepairTaskCommandValidator : AbstractValidator<RemoveRepairTaskCommand>
    {
        public RemoveRepairTaskCommandValidator()
        {
            RuleFor(x => x.RepairTaskId).NotEmpty()
                .WithErrorCode(RepairTaskErrors.IdRequired.Code)
                .WithMessage(RepairTaskErrors.IdRequired.Description);
        }
    }
}
