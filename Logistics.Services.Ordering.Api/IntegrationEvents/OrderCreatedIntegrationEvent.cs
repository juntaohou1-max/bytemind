namespace Logistics.Services.Ordering.Api.IntegrationEvents
{
    public class OrderCreatedIntegrationEvent : IntegrationEvent
    {
        public OrderCreatedIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string customerId,
            IReadOnlyCollection<OrderCreatedIntegrationEventLine> lines)
            : base(
                "OrderCreatedIntegrationEvent",
                tenantId,
                aggregateId,
                occurredAt)
        {
            ExternalOrderNo = externalOrderNo;
            CustomerId = customerId;
            Lines = lines;
        }

        public string ExternalOrderNo { get; }

        public string CustomerId { get; }

        public IReadOnlyCollection<OrderCreatedIntegrationEventLine> Lines { get; }
    }

    public class OrderCreatedIntegrationEventLine
    {
        public OrderCreatedIntegrationEventLine(
            string skuId,
            int quantity)
        {
            SkuId = skuId;
            Quantity = quantity;
        }

        public string SkuId { get; }

        public int Quantity { get; }
    }
}
