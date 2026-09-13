using MechanicShop.Application.Common.Extensions;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers
{
    public sealed class GetCustomersQueryHandler(IAppDbContext context) : IRequestHandler<GetCustomersQuery, Result<PaginatedList<CustomerListItemDto>>>
    {
        private readonly IAppDbContext _context = context;
        public async Task<Result<PaginatedList<CustomerListItemDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
        {
            //not optimal way 
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderByDescending(c=>c.CreatedAtUtc)
                .Select(c => new CustomerListItemDto(c.Id, c.Name!, c.PhoneNumber!, c.Email!, c.Vehicles.Count()))
                .ToPaginatedListAsync(request.Page, request.PageSize, ct);

            return customers;
        }
    }
}
