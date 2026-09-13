using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Labors.Dtos;
using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Features.Labors.Queries.GetLabors
{
    public sealed class GetLaborsQueryHandler(IAppDbContext context) : IRequestHandler<GetLaborsQuery, Result<List<LaborDto>>>
    {
        private readonly IAppDbContext _context = context;

        public async Task<Result<List<LaborDto>>> Handle(GetLaborsQuery query, CancellationToken ct)
        {
            // Materialize entities first then map to DTOs to avoid any EF translation issues for the mapper
            var employees = await _context.Employees.AsNoTracking()
                .Where(e => e.Role == Role.Labor)
                .ToListAsync(ct);

            var dtos = employees.Select(e => e.ToDto()).ToList();

            return dtos;
        }
    }
}
