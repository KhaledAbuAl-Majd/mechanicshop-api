using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Customers
{
    public static class CustomerErrors
    {
        public static Error IdRequired =>
            Error.Validation("Customer.Id.Required", "Customer ID is required.");

        public static Error NameRequired =>
            Error.Validation("Customer.Name.Required", "Customer name is required.");

        public static Error PhoneNumberRequired =>
       Error.Validation("Customer.Number.Required", "Phone number is required");

        public static Error EmailRequired =>
            Error.Validation("Customer.Email.Required", "Email is required");

        public static Error EmailInvalid =>
            Error.Validation("Customer.Email.Invalid", "Email is invalid");

        public static Error CustomerEmailExists =>
       Error.Conflict("Customer.Email.Exists", "A customer with this email already exists.");

        public static readonly Error InvalidPhoneNumber =
        Error.Conflict("Customer.InvalidPhoneNumber", "Phone number must be 7–15 digits and may start with '+'.");


        public static readonly Error CannotDeleteCustomerWithWorkOrders =
            Error.Conflict("Customer.CannotDelete", "Customer cannot be deleted due to existing work orders.");
    }
}
