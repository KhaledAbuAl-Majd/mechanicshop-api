using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.RemoveCustomer
{
    public sealed class RemoveCustomerCommandHandler(
        ILogger<RemoveCustomerCommandHandler> logger,
        IAppDbContext context) : IRequestHandler<RemoveCustomerCommand, Result<Deleted>>
    {

        private readonly ILogger<RemoveCustomerCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<Deleted>> Handle(RemoveCustomerCommand command, CancellationToken ct)
        {
            var customer = await _context.Customers.FindAsync([command.CustomerId], ct);

            if (customer is null)
            {
                _logger.LogWarning("Customer with id {CustomerId} not found for deletion.", command.CustomerId);
                return ApplicationErrors.CustomerNotFound;
            }

            //var hasAssociatedWorkOrders = await _context.WorkOrders
            //    .Include(v=>v.Vehicle)
            //    .Where(wo => wo.Vehicle != null).AnyAsync(wo => wo.Vehicle!.CustomerId == command.CustomerId, ct);

            var hasAssociatedWorkOrders = await _context.WorkOrders
                .AnyAsync(wo => wo.Vehicle!.CustomerId == command.CustomerId, ct);


            if (hasAssociatedWorkOrders)
            {
                _logger.LogWarning("Customer {CustomerId} cannot be deleted because they have associated work orders (past, scheduled, or in-progress).", command.CustomerId);
                return CustomerErrors.CannotDeleteCustomerWithWorkOrders;
            }


            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Customer {CustomerId} deleted successfully.", command.CustomerId);

            return Result.Deleted;
        }
    }
}
