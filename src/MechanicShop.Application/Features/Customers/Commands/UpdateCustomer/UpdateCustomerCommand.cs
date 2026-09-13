using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer
{
    public sealed record UpdateCustomerCommand(
        Guid CustomerId,
        string Name,
        string PhoneNumber,
        string Email,
        List<UpdateVehicleCommand> Vehicles

    ) : IInvalidateCacheCommand<Result<Updated>>

    {
        public string[] Tags => [CustomerCache.Tag];
    }
}
