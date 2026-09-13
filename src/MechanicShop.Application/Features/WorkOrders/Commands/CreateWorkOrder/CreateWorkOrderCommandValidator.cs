using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder
{
    public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
    {
        public CreateWorkOrderCommandValidator()
        {
            RuleFor(x => x.VehicleId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.VehicleIdRequired.Code)
                .WithMessage(WorkOrderErrors.VehicleIdRequired.Description);

            RuleFor(x => x.StartAt).GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("StartAt must be in the future.");

            RuleFor(x => x.RepairTaskIds).NotEmpty()
            .WithMessage("At least one repair task must be selected");

            //RuleFor(x => x.LaborId).Must(laborId => laborId is null || laborId != Guid.Empty)
            //    .WithMessage("if provided, LaborId must be not empty.");

            RuleFor(x => x.LaborId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.LaborIdRequired.Code)
                .WithMessage(WorkOrderErrors.LaborIdRequired.Description);

            RuleFor(x => x.Spot).IsInEnum()
                .WithErrorCode(WorkOrderErrors.SpotInvalid.Code)
                .WithMessage("Spot must be a valid Spot value. [A, B, C, D]");
        }
    }
}
