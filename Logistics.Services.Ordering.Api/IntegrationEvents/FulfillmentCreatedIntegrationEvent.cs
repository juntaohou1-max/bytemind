namespace Logistics.Services.Ordering.Api.IntegrationEvents
{
    public class FulfillmentCreatedIntegrationEvent : IntegrationEvent
    {
        public FulfillmentCreatedIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string status)
            : base(
                "FulfillmentCreatedIntegrationEvent",
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
