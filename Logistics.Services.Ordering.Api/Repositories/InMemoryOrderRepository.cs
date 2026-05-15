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

        public Task<IReadOnlyCollection<Order>> SearchAsync(OrderQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            IEnumerable<Order> orders = _orders.Values;

            if (query.Status.HasValue)
            {
                orders = orders.Where(order => order.Status == query.Status.Value);
            }

            if (query.From.HasValue)
            {
                orders = orders.Where(order => order.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                orders = orders.Where(order => order.CreatedAt <= query.To.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.ExternalOrderNo))
            {
                var externalOrderNo = query.ExternalOrderNo.Trim();

                orders = orders.Where(order => order.ExternalOrderNo == externalOrderNo);
            }

            return Task.FromResult<IReadOnlyCollection<Order>>(orders.ToList());
        }

        public Task<Order?> GetByIdAsync(Guid id)
        {
            _orders.TryGetValue(id, out var order);

            return Task.FromResult(order);
        }

        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }
    }
}
