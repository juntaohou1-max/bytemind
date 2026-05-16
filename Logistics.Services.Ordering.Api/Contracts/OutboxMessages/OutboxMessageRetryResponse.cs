namespace Logistics.Services.Ordering.Api.Contracts.OutboxMessages
{
    public class OutboxMessageRetryResponse
    {
        public Guid Id { get; init; }

        public string Status { get; init; } = string.Empty;

        public int RetryCount { get; init; }
    }
}
