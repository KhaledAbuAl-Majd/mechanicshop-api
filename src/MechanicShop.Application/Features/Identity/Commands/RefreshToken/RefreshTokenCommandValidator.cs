using FluentValidation;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty()
                .WithErrorCode("RefreshToken.Required")
                .WithMessage("Refresh TokenHash is required.");

            RuleFor(x => x.ExpiredAccessToken).NotEmpty()
                .WithErrorCode("ExpiredAccessToken.Required")
                .WithMessage("Expired Access TokenHash is required.");
        }
    }
}
