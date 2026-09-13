using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100);

            RuleFor(x => x.Email).EmailAddress()
                .WithMessage("Invalid email")
                .MaximumLength(100);

            RuleFor(x => x.PhoneNumber).NotEmpty()
                .WithMessage("Phone Number is required.")
                .Matches(@"^\+?\d{7,15}$")
                .WithMessage("Phone number must be 7–15 digits and may start with '+'.");

            //RuleFor(x => x.Vehicles).NotNull()
            //    .WithMessage("Vehilce list cannot be null")
            //    .Must(p => p.Count > 0)
            //    .WithMessage("At least one vehicle is required.");

            RuleFor(x => x.Vehicles)
            .NotEmpty().WithMessage("At least one vehicle is required.");

            RuleForEach(x => x.Vehicles).SetValidator(new CreateVehicleCommandValidator());//validate each vehicle with it validator 
        }
    }
}
