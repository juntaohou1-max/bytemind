using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace Logistics.Services.Ordering.Api.Application.Orders
{
    public class OrderApplicationService : IOrderApplicationService
    {
        private const string OrderCancelledEventType = "OrderCancelled";

        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxMessageRepository _outboxMessageRepository;

        public OrderApplicationService(IOrderRepository orderRepository, IOutboxMessageRepository outboxMessageRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            //构造函数参数不能为空，如果传进来是 null，就立刻抛异常。
            _outboxMessageRepository = outboxMessageRepository ?? throw new ArgumentNullException(nameof(outboxMessageRepository));
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                return false;

            var originalStatus = order.Status;

            order.Cancel();

            if (order.Status != originalStatus)
            {
                var occurredAt = DateTimeOffset.UtcNow;

                var payload = JsonSerializer.Serialize(new
                {
                    OrderId = order.Id,
                    order.TenantId,
                    order.ExternalOrderNo,
                    Status = order.Status.ToString(),
                    OccurredAt = occurredAt
                });

                await _outboxMessageRepository.AddAsync(new OutboxMessage(
                    OrderCancelledEventType,
                    payload,
                    occurredAt));
            }

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

            try
            {
                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueIndexConflict(ex))
            {
                var savedOrder = await _orderRepository.GetByTenantAndExternalOrderNoAsync(
                    request.TenantId!,
                    request.ExternalOrderNo!);

                if (savedOrder is not null)
                {
                    return new CreateOrderResponse
                    {
                        Id = savedOrder.Id
                    };
                }
                throw;
            }

            return new CreateOrderResponse
            {
                Id = order.Id
            };
        }

        public async Task<PagedResponse<OrderListItemResponse>> GetAllAsync(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo,
            int pageNumber,
            int pageSize,
            string sort = "createdAtDesc")
        {
            OrderStatus? orderStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsedStatus))
                    throw new ArgumentException("订单状态不正确。", nameof(status));

                orderStatus = parsedStatus;
            }

            if (pageNumber < 1)
                throw new ArgumentException("页码必须大于等于 1。", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("每页数量必须大于等于 1。", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("每页数量不能超过 100。", nameof(pageSize));

            if (!string.Equals(sort, "createdAtDesc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sort, "createdAtAsc", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("排序字段不正确。", nameof(sort));
            }

            var orders = await _orderRepository.SearchAsync(new OrderQuery
            {
                Status = orderStatus,
                From = from,
                To = to,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Sort = sort,
                ExternalOrderNo = externalOrderNo
            });

            var items = orders.Items
                .Select(OrderContractMapper.ToListItemResponse)
                .ToList();

            return OrderContractMapper.ToPagedResponse(
                items,
                pageNumber,
                pageSize,
                orders.TotalCount);
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

        private static bool IsUniqueIndexConflict(DbUpdateException exception)
        {
            return exception.InnerException?.Message.Contains(
                "IX_Orders_TenantId_ExternalOrderNo",
                StringComparison.OrdinalIgnoreCase) == true;
        }

    }
}
