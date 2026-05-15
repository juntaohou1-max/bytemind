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
        void Add(Order order);

        /// <summary>
        /// 根据订单 ID 查询订单。
        /// </summary>
        /// <param name="id">订单 ID。</param>
        /// <returns>找到则返回订单，找不到则返回 null。</returns>
        Order? GetById(Guid id);

        /// <summary>
        /// 查询所有订单。
        /// </summary>
        /// <returns>订单集合。</returns>
        IReadOnlyCollection<Order> GetAll();


    }
}
