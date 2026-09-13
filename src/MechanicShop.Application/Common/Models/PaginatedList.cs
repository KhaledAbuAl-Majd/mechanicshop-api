namespace MechanicShop.Application.Common.Models
{
    public sealed class PaginatedList<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        public IReadOnlyList<T> Items { get; } = items;
        public int Page { get; } = page;
        public int PageSize { get; } = pageSize;
        public int TotalCount { get; } = totalCount;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
