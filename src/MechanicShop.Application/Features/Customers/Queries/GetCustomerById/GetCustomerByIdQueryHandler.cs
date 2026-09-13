using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed class GetCustomerByIdQueryHandler(
        ILogger<GetCustomerByIdQueryHandler> logger,
        IAppDbContext context) : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
    {
        private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;
        private readonly IAppDbContext _context = context;
        public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
        {
            var customer = await _context.Customers.AsNoTracking().Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == query.CustomerId, ct);

            if (customer is null)
            {
                _logger.LogWarning("Customer with id {CustomerId} was not found", query.CustomerId);

                var code = ApplicationErrors.CustomerNotFound.Code;
                return Error.NotFound(
               code: code,
               description: $"Customer with id '{query.CustomerId}' was not found");

                //return ApplicationErrors.CustomerNotFound;
            }

            return customer.ToDto();
        }
    }
}
