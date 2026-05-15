using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<Guid, Order> _orders = new();

        public void Add(Order order)
        {
            _orders[order.Id] = order;
        }

        public IReadOnlyCollection<Order> GetAll()
        {
            return _orders.Values.ToList();
        }

        public Order? GetById(Guid id)
        {
            _orders.TryGetValue(id, out var order);
            return order;
        }
    }
}
