namespace Logistics.Services.Ordering.Api.IntegrationEvents
{
    public class InventoryReservedIntegrationEvent : IntegrationEvent
    {
        public InventoryReservedIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string status)
            : base(
                "InventoryReservedIntegrationEvent",
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
