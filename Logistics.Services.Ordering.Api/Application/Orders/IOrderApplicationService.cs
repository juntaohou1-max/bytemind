using Logistics.Services.Ordering.Api.Contracts.Orders;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    public interface IOrderApplicationService
    {
        CreateOrderResponse Create(CreateOrderRequest request);

        OrderDetailResponse? GetById(Guid id);

        IReadOnlyCollection<OrderListItemResponse> GetAll(
            string? status,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? externalOrderNo);

        bool Cancel(Guid id);

        IReadOnlyCollection<OrderTimelineItemResponse>? GetTimeline(Guid id);

        bool MarkInventoryReserved(Guid id);

        bool MarkFulfillmentCreated(Guid id);
    }
}
