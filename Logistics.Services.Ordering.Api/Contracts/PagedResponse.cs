namespace Logistics.Services.Ordering.Api.Contracts
{
    /// <summary>
    /// Common response shape for paged list endpoints.
    /// </summary>
    /// <typeparam name="T">Single item response type.</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Items in the current page.
        /// </summary>
        public IReadOnlyCollection<T> Items { get; init; } = [];

        /// <summary>
        /// Current page number, starting from 1.
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// Number of items requested for each page.
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Total item count matching the query.
        /// </summary>
        public int TotalCount { get; init; }

        /// <summary>
        /// Total page count calculated from TotalCount and PageSize.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
