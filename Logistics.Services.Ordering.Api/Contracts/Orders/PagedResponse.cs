namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    /// <summary>
    /// 分页接口的统一响应结构。
    /// </summary>
    /// <typeparam name="T">当前页中单条数据的响应类型。</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 当前页的数据集合。
        /// </summary>
        public IReadOnlyCollection<T> Items { get; init; } = [];

        /// <summary>
        /// 当前页码，从 1 开始。
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// 每页数据条数。
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// 符合查询条件的总数据条数。
        /// </summary>
        public int TotalCount { get; init; }

        /// <summary>
        /// 根据总条数和每页条数计算出的总页数。
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
