using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerCommandHandler(ILogger<UpdateCustomerCommandHandler> logger, IAppDbContext context) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
    {

        private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken ct)
        {
            var customer = await _context.Customers.Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);

            if (customer is null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for update.", command.CustomerId);

                return ApplicationErrors.CustomerNotFound;
            }

            var email = command.Email.Trim().ToLower();

            if (!customer.Email!.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _context.Customers.AnyAsync(c => c.Id != customer.Id && c.Email!.ToLower() == email, ct);

                if (exists)
                {
                    _logger.LogWarning("Customer updating aborted. Email already exists.");

                    return CustomerErrors.CustomerEmailExists;
                }
            }


            var validatedVehicles = new List<Vehicle>();

            //add all and the exists will removed at domain by upsert
            foreach (var v in command.Vehicles)
            {
                var vehicleId = v.VehicleId ?? Guid.NewGuid();

                var result = Vehicle.Create(vehicleId, v.Make, v.Model, v.Year, v.LicensePlate);

                if (result.IsError)
                    return result.Errors;

                validatedVehicles.Add(result.Value);
            }

            var updateCustomerResult = customer.Update(command.Name, command.Email, command.PhoneNumber);

            if (updateCustomerResult.IsError)
                return updateCustomerResult.Errors;

            var upsertPartsResult = customer.UpsertVehicles(validatedVehicles);

            if (upsertPartsResult.IsError)
                return upsertPartsResult.Errors;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Customer updated successfully. Id: {CustomerId}", customer.Id);

            return Result.Updated;
        }
    }
}
