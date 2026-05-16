namespace Logistics.Services.Ordering.Api.IntegrationEvents
{
    public class OrderCancelledIntegrationEvent : IntegrationEvent
    {
        public OrderCancelledIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string status)
            : base(
                "OrderCancelledIntegrationEvent",
                tenantId,
                aggregateId,
                occurredAt)
        {
            ExternalOrderNo = externalOrderNo;
            Status = status;
        }

        public string ExternalOrderNo { get; }

        public string Status { get; }
    }
}
