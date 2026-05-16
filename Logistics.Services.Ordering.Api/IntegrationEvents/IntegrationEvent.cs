namespace Logistics.Services.Ordering.Api.IntegrationEvents
{
    public abstract class IntegrationEvent
    {
        protected IntegrationEvent(
            string eventType,
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt)
        {
            EventId = Guid.NewGuid();
            EventType = eventType;
            TenantId = tenantId;
            AggregateId = aggregateId;
            OccurredAt = occurredAt;
            Version = 1;
        }

        public Guid EventId { get; }

        public string EventType { get; }

        public DateTimeOffset OccurredAt { get; }

        public string? TraceId { get; init; }

        public string TenantId { get; }

        public string AggregateId { get; }

        public int Version { get; }
    }
}
