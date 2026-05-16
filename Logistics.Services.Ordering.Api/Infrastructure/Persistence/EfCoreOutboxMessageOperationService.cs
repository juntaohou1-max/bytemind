using Logistics.Services.Ordering.Api.Application.OutboxMessages;
using Logistics.Services.Ordering.Api.Contracts.OutboxMessages;
using Logistics.Services.Ordering.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    public class EfCoreOutboxMessageOperationService : IOutboxMessageOperationService
    {
        private readonly OrderingDbContext _dbContext;

        public EfCoreOutboxMessageOperationService(OrderingDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<OutboxMessageRetryResponse?> RetryAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var message = await _dbContext.OutboxMessages
                .SingleOrDefaultAsync(message => message.Id == id, cancellationToken);

            if (message is null)
            {
                return null;
            }

            if (message.Status != OutboxStatus.Failed)
            {
                throw new InvalidOperationException("Only failed Outbox messages can be retried manually.");
            }

            message.MarkPendingForRetry();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ToRetryResponse(message);
        }

        public async Task<OutboxMessagesRetryResponse> RetryFailedAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            ValidateTake(take);

            var messages = await _dbContext.OutboxMessages
                .Where(message => message.Status == OutboxStatus.Failed)
                .OrderBy(message => message.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                message.MarkPendingForRetry();
            }

            if (messages.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new OutboxMessagesRetryResponse
            {
                RetriedCount = messages.Count,
                Items = messages
                    .Select(ToRetryResponse)
                    .ToList()
            };
        }

        private static void ValidateTake(int take)
        {
            if (take < 1)
            {
                throw new ArgumentException("Take must be greater than or equal to 1.", nameof(take));
            }

            if (take > 100)
            {
                throw new ArgumentException("Take cannot be greater than 100.", nameof(take));
            }
        }

        private static OutboxMessageRetryResponse ToRetryResponse(OutboxMessage message)
        {
            return new OutboxMessageRetryResponse
            {
                Id = message.Id,
                Status = message.Status.ToString(),
                RetryCount = message.RetryCount
            };
        }
    }
}
