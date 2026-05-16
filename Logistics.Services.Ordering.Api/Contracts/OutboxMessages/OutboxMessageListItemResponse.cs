namespace Logistics.Services.Ordering.Api.Contracts.OutboxMessages
{
    public class OutboxMessageListItemResponse
    {
        public Guid Id { get; init; }

        public string EventType { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public DateTimeOffset OccurredAt { get; init; }

        public DateTimeOffset? ProcessedAt { get; init; }

        public int RetryCount { get; init; }
    }
}
