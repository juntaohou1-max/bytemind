using Logistics.Services.Ordering.Api.Contracts.OutboxMessages;

namespace Logistics.Services.Ordering.Api.Application.OutboxMessages
{
    public interface IOutboxMessageOperationService
    {
        Task<OutboxMessageRetryResponse?> RetryAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<OutboxMessagesRetryResponse> RetryFailedAsync(
            int take,
            CancellationToken cancellationToken = default);
    }
}
