using Logistics.Services.Ordering.Api.Contracts.Orders;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    public interface IOrderApplicationService
    {
        /// <summary>
        /// 创建订单；同一租户下重复的外部订单号会返回已有订单。
        /// </summary>
        /// <param name="request">创建订单请求。</param>
        /// <returns>订单创建结果。</returns>
        Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request);

        /// <summary>
        /// 根据订单 ID 查询订单详情。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到则返回订单详情，找不到则返回 null。</returns>
        Task<OrderDetailResponse?> GetByIdAsync(Guid id);

        /// <summary>
        /// 按条件分页查询订单列表。
        /// </summary>
        /// <param name="status">订单状态过滤条件。</param>
        /// <param name="from">创建时间起始边界。</param>
        /// <param name="to">创建时间结束边界。</param>
        /// <param name="externalOrderNo">外部系统订单号。</param>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数据条数。</param>
        /// <param name="sort">排序方式，当前支持 createdAtDesc 和 createdAtAsc。</param>
        /// <returns>分页后的订单列表响应。</returns>
        Task<Logistics.Services.Ordering.Api.Contracts.PagedResponse<OrderListItemResponse>> GetAllAsync(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo,
            int pageNumber,
            int pageSize,
            string sort);

        /// <summary>
        /// 取消订单。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到并处理则返回 true，找不到则返回 false。</returns>
        Task<bool> CancelAsync(Guid id);

        /// <summary>
        /// 查询订单时间线。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到则返回时间线集合，找不到则返回 null。</returns>
        Task<IReadOnlyCollection<OrderTimelineItemResponse>?> GetTimelineAsync(Guid id);

        /// <summary>
        /// 标记订单库存已锁定。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到并处理则返回 true，找不到则返回 false。</returns>
        Task<bool> MarkInventoryReservedAsync(Guid id);

        /// <summary>
        /// 标记订单履约单已创建。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到并处理则返回 true，找不到则返回 false。</returns>
        Task<bool> MarkFulfillmentCreatedAsync(Guid id);
    }
}
