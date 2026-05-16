namespace Logistics.Services.Ordering.Api.Contracts.OutboxMessages
{
    public class OutboxMessagesRetryResponse
    {
        public int RetriedCount { get; init; }

        public IReadOnlyCollection<OutboxMessageRetryResponse> Items { get; init; } = [];
    }
}
