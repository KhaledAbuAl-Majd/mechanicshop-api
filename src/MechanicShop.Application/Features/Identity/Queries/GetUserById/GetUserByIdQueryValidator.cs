using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserById
{
    public sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty()
                .WithErrorCode("User.Id.Required")
                .WithMessage("User ID is required.");
        }
    }
}
