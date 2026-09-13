using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken
{
    public sealed class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
    {
        public GenerateTokenCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty()
                .WithErrorCode("Email.Required")
                .WithMessage("Email is required")
                .EmailAddress()
                .WithErrorCode("Email.Invalid")
                .WithMessage("Email is invalid.");

            RuleFor(x => x.Password).NotEmpty()
                .WithErrorCode("Password.Required")
                .WithMessage("Password is required.");
        }
    }
}
