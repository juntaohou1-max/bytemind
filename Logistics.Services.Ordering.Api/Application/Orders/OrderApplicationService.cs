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
            var existingOrder = await _orderRepository.GetByTenantAndExternalOrderNoAsync(
                request.TenantId!,
                request.ExternalOrderNo!);

            if (existingOrder is not null)
                return new CreateOrderResponse
                {
                    Id = existingOrder.Id
                };

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
            OrderStatus? orderStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsedStatus))
                    throw new ArgumentException("订单状态不正确。", nameof(status));

                orderStatus = parsedStatus;
            }

            var orders = await _orderRepository.SearchAsync(new OrderQuery
            {
                Status = orderStatus,
                From = from,
                To = to,
                ExternalOrderNo = externalOrderNo
            });

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
