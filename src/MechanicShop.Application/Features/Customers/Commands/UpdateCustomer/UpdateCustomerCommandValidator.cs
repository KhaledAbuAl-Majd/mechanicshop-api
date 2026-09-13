using FluentValidation;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerCommandValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer Id is required.");

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

            RuleFor(x => x.Vehicles)
            .NotEmpty().WithMessage("At least one vehicle is required.");

            RuleForEach(x => x.Vehicles).SetValidator(new UpdateVehicleCommandValidator());//validate each vehicle with it validator 
        }
    }
}
