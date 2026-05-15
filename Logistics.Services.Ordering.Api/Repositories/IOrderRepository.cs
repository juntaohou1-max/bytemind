using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    public interface IOrderRepository
    {
        /// <summary>
        /// 新增订单。
        /// </summary>
        /// <param name="order">要保存的订单。</param>
        Task AddAsync(Order order);

        /// <summary>
        /// 根据订单 ID 查询订单。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到则返回订单，找不到则返回 null。</returns>
        Task<Order?> GetByIdAsync(Guid id);

        /// <summary>
        /// 按条件分页查询订单。
        /// </summary>
        /// <param name="query">订单查询条件。</param>
        /// <returns>当前页订单集合和符合条件的总数。</returns>
        Task<PagedResult<Order>> SearchAsync(OrderQuery query);

        /// <summary>
        /// 保存已跟踪实体的变更。
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// 根据租户和外部订单号查询订单，用于创建订单时做幂等判断。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="externalOrderNo">外部系统订单号。</param>
        /// <returns>找到则返回订单，找不到则返回 null。</returns>
        Task<Order?> GetByTenantAndExternalOrderNoAsync(string tenantId, string externalOrderNo);
    }
}
