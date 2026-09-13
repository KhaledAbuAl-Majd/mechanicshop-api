using FluentValidation;
using MechanicShop.Domain.RepairTasks;

namespace MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask
{
    public class UpdateRepairTaskCommandValidator : AbstractValidator<UpdateRepairTaskCommand>
    {
        public UpdateRepairTaskCommandValidator()
        {
            RuleFor(x => x.RepairTaskId).NotEmpty()
                .WithErrorCode(RepairTaskErrors.IdRequired.Code)
                .WithMessage(RepairTaskErrors.IdRequired.Description);

            RuleFor(x => x.Name).NotEmpty()
                .WithErrorCode(RepairTaskErrors.NameRequired.Code)
                .WithMessage(RepairTaskErrors.NameRequired.Description)
                .MaximumLength(100);

            RuleFor(x => x.LaborCost).GreaterThan(0)
                .WithMessage("Labor cost must be greater than 0.");

            RuleFor(x => x.EstimatedDurationInMins)
                    .NotNull()
                    .WithMessage("Estimated duration is required.")
                    .IsInEnum();

            RuleFor(x => x.Parts)
                    .NotNull().WithMessage("Parts list cannot be null.")
                    .Must(p => p?.Count > 0).WithMessage("At least one part is required.");

            RuleForEach(x => x.Parts).SetValidator(new UpdateRepairTaskPartCommandValidator());
        }
    }
}
