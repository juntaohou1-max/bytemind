using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    public class OrderApplicationService : IOrderApplicationService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderApplicationService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return false;

            order.Cancel();

            await _orderRepository.SaveChangesAsync();

            return true;
        }

        public async Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request)
        {
            var order = OrderContractMapper.ToOrder(request);

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return new CreateOrderResponse
            {
                Id = order.Id
            };
        }

        public async Task<IReadOnlyCollection<OrderListItemResponse>> GetAllAsync(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo)
        {
            var orders = await _orderRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var orderStatus))
                    throw new ArgumentException("订单状态不正确。", nameof(status));

                orders = orders
                    .Where(order => order.Status == orderStatus)
                    .ToList();
            }

            if (from.HasValue)
            {
                orders = orders
                    .Where(order => order.CreatedAt >= from.Value)
                    .ToList();
            }

            if (to.HasValue)
            {
                orders = orders
                    .Where(order => order.CreatedAt <= to.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(externalOrderNo))
            {
                orders = orders
                    .Where(order => order.ExternalOrderNo == externalOrderNo)
                    .ToList();
            }

            return orders
                .Select(OrderContractMapper.ToListItemResponse)
                .ToList();
        }

        public async Task<OrderDetailResponse?> GetByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return null;

            return OrderContractMapper.ToDetailResponse(order);
        }

        public async Task<IReadOnlyCollection<OrderTimelineItemResponse>?> GetTimelineAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return null;

            return OrderContractMapper.ToTimelineResponse(order);
        }

        public async Task<bool> MarkFulfillmentCreatedAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return false;

            order.MarkFulfillmentCreated();

            await _orderRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkInventoryReservedAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return false;

            order.MarkInventoryReserved();

            await _orderRepository.SaveChangesAsync();

            return true;
        }
    }
}
