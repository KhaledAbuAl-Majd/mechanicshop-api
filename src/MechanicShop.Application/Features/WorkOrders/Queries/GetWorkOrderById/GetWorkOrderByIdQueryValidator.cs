using FluentValidation;
using MechanicShop.Domain.WorkOrders;

namespace MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderById
{
    public sealed class GetWorkOrderByIdQueryValidator : AbstractValidator<GetWorkOrderByIdQuery>
    {
        public GetWorkOrderByIdQueryValidator()
        {
            RuleFor(x => x.WorkOrderId).NotEmpty()
                .WithErrorCode(WorkOrderErrors.WorkOrderIdRequired.Code)
                .WithMessage(WorkOrderErrors.WorkOrderIdRequired.Description);
        }
    }
}
