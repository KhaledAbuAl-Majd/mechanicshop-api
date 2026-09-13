using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Constants;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer
{
    public sealed record RemoveCustomerCommand(Guid CustomerId) : IInvalidateCacheCommand<Result<Deleted>>
    {
        public string[] Tags => [CustomerCache.Tag];
    }
}
