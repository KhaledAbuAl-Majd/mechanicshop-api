using FluentValidation;

namespace MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats
{
    public sealed class GetWorkOrderStatsQueryValidator : AbstractValidator<GetWorkOrderStatsQuery>
    {
        public GetWorkOrderStatsQueryValidator()
        {
            RuleFor(request => request.Date)
              .NotEmpty()
              .WithErrorCode("Date.Is.Required")
              .WithMessage("Date is required.");
        }
    }
}
