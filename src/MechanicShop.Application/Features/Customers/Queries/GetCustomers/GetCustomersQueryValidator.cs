using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
    {
        public GetCustomersQueryValidator()
        {
            RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("Page.Number.Invalid")
            .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithErrorCode("Page.Size.Invalid")
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
