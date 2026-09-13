using MechanicShop.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MechanicShop.Application.Common.Extensions
{
    public static class MappingExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int page, int pageSize, CancellationToken ct = default)
        {
            var count = await source.CountAsync(ct);

            var items = count == 0 ? [] : await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PaginatedList<T>(items, page, pageSize, count);
        }
    }
}
