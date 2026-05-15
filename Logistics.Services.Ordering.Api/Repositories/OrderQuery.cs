using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    /// <summary>
    /// 订单列表查询条件。
    /// </summary>
    public class OrderQuery
    {
        /// <summary>
        /// 订单状态过滤条件；为 null 时不按状态过滤。
        /// </summary>
        public OrderStatus? Status { get; init; }

        /// <summary>
        /// 创建时间起始边界；为 null 时不限制起始时间。
        /// </summary>
        public DateTimeOffset? From { get; init; }

        /// <summary>
        /// 创建时间结束边界；为 null 时不限制结束时间。
        /// </summary>
        public DateTimeOffset? To { get; init; }

        /// <summary>
        /// 外部系统订单号过滤条件；为 null 或空白时不按外部单号过滤。
        /// </summary>
        public string? ExternalOrderNo { get; init; }

        /// <summary>
        /// 页码，从 1 开始。
        /// </summary>
        public int PageNumber { get; init; } = 1;

        /// <summary>
        /// 每页数据条数。
        /// </summary>
        public int PageSize { get; init; } = 20;

        /// <summary>
        /// 排序方式；当前支持 createdAtDesc 和 createdAtAsc。
        /// </summary>
        public string Sort { get; init; } = "createdAtDesc";
    }
}
