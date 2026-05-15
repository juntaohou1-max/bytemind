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
        /// 按条件查询订单。
        /// </summary>
        /// <param name="query">订单查询条件。</param>
        /// <returns>订单集合。</returns>
        Task<IReadOnlyCollection<Order>> SearchAsync(OrderQuery query);

        /// <summary>
        /// 保存已跟踪实体的变更。
        /// </summary>
        Task SaveChangesAsync();
    }
}
