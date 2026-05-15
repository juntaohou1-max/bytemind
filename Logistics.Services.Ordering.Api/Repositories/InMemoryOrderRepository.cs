using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly Dictionary<Guid, Order> _orders = new();

        public Task AddAsync(Order order)
        {
            _orders[order.Id] = order;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Order>> GetAllAsync()
        {
            IReadOnlyCollection<Order> orders = _orders.Values.ToList();

            return Task.FromResult(orders);
        }

        public Task<Order?> GetByIdAsync(Guid id)
        {
            _orders.TryGetValue(id, out var order);

            return Task.FromResult(order);
        }
    }
}
