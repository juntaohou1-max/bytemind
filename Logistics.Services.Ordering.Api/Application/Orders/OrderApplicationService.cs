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

        public bool Cancel(Guid id)
        {
            var order = _orderRepository.GetById(id);

            if (order is null)
                return false;

            order.Cancel();

            return true;
        }

        public CreateOrderResponse Create(CreateOrderRequest request)
        {
            var order = OrderContractMapper.ToOrder(request);

            _orderRepository.Add(order);

            return new CreateOrderResponse
            {
                Id = order.Id
            };
        }

        public IReadOnlyCollection<OrderDetailResponse> GetAll(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo)
        {
            var orders = _orderRepository.GetAll();

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
                .Select(OrderContractMapper.ToDetailResponse)
                .ToList();
        }

        public OrderDetailResponse? GetById(Guid id)
        {
            var order = _orderRepository.GetById(id);

            if (order is null)
                return null;

            return OrderContractMapper.ToDetailResponse(order);
        }

        public IReadOnlyCollection<OrderTimelineItemResponse>? GetTimeline(Guid id)
        {
            var order = _orderRepository.GetById(id);

            if (order is null)
                return null;

            return OrderContractMapper.ToTimelineResponse(order);
        }
    }
}
