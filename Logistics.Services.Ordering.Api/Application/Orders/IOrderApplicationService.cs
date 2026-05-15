using Logistics.Services.Ordering.Api.Contracts.Orders;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    public interface IOrderApplicationService
    {
        Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request);

        Task<OrderDetailResponse?> GetByIdAsync(Guid id);

        Task<IReadOnlyCollection<OrderListItemResponse>> GetAllAsync(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo);

        Task<bool> CancelAsync(Guid id);

        Task<IReadOnlyCollection<OrderTimelineItemResponse>?> GetTimelineAsync(Guid id);

        Task<bool> MarkInventoryReservedAsync(Guid id);

        Task<bool> MarkFulfillmentCreatedAsync(Guid id);
    }
}
