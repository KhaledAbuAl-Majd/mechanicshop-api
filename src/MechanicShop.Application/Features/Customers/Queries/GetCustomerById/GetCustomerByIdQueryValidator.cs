using FluentValidation;
using MechanicShop.Domain.Customers;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
    {
        public GetCustomerByIdQueryValidator()
        {
            RuleFor(request => request.CustomerId)
          .NotEmpty()
          .WithErrorCode(CustomerErrors.IdRequired.Code)
          .WithMessage(CustomerErrors.IdRequired.Description);
        }
    }
}
