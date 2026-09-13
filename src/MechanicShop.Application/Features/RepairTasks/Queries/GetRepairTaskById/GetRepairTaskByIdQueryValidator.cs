using FluentValidation;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById
{
    public sealed class GetRepairTaskByIdQueryValidator : AbstractValidator<GetRepairTaskByIdQuery>
    {
        public GetRepairTaskByIdQueryValidator()
        {
            RuleFor(x => x.RepairTaskId).NotEmpty()
                .WithErrorCode(RepairTaskErrors.IdRequired.Code)
                .WithMessage(RepairTaskErrors.IdRequired.Description);
        }
    }
}
