using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Commands.CreateCustomer
{
    public sealed record CreateCustomerCommand(string Name, string PhoneNumber, string Email, List<CreateVehicleCommand> Vehicles)
        : IInvalidateCacheCommand<Result<CustomerDto>>
    {
        public string[] Tags => [CustomerCache.Tag];
    }
}
