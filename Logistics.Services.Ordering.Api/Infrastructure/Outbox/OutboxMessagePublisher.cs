using Logistics.Services.Ordering.Api.Application.IntegrationEvents;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Ordering.Api.Infrastructure.Outbox
{
    /// <summary>
    /// Background worker that retries failed Outbox messages and publishes pending messages.
    /// </summary>
    /// <remarks>
    /// The worker owns scheduling and status transitions. Actual delivery is delegated to
    /// IIntegrationEventPublisher so RabbitMQ, Kafka, or HTTP delivery can be added later.
    /// </remarks>
    public class OutboxMessagePublisher : BackgroundService
    {
        // Limit one polling round so the worker does not load too much data at once.
        private const int BatchSize = 20;

        // Failed messages stop automatic retry after this count and wait for manual handling.
        private const int MaxRetryCount = 3;

        // Polling interval can be moved to configuration later.
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        public OutboxMessagePublisher(
            IServiceScopeFactory scopeFactory,
            IIntegrationEventPublisher eventPublisher,
            ILogger<OutboxMessagePublisher> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Runs one Outbox processing round. Tests call this directly instead of starting the endless worker loop.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task ProcessOnceAsync(CancellationToken cancellationToken)
        {
            await RequeueFailedMessagesAsync(cancellationToken);
            await PublishPendingMessagesAsync(cancellationToken);
        }

        /// <summary>
        /// Entry point called by ASP.NET Core when the hosted service starts.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token triggered when the host is stopping.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Outbox publisher execution failed.");
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Moves retryable failed messages back to Pending.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task RequeueFailedMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var outboxMessageRepository = scope.ServiceProvider
                .GetRequiredService<IOutboxMessageRepository>();

            var messages = await outboxMessageRepository.GetFailedMessagesForRetryAsync(
                MaxRetryCount,
                BatchSize,
                cancellationToken);

            var requeuedCount = 0;

            foreach (var message in messages)
            {
                if (!message.CanRetry(MaxRetryCount))
                {
                    continue;
                }

                message.MarkPendingForRetry();
                requeuedCount++;
            }

            if (requeuedCount > 0)
            {
                await outboxMessageRepository.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Loads and publishes one batch of pending messages.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var outboxMessageRepository = scope.ServiceProvider
                .GetRequiredService<IOutboxMessageRepository>();

            var messages = await outboxMessageRepository.GetMessagesByStatusAsync(
                OutboxStatus.Pending,
                BatchSize,
                cancellationToken);

            foreach (var message in messages)
            {
                await PublishMessageAsync(message, cancellationToken);
            }

            if (messages.Count > 0)
            {
                await outboxMessageRepository.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Publishes one message and updates its state based on the result.
        /// </summary>
        /// <param name="message">Outbox message to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task PublishMessageAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            try
            {
                message.MarkPublishing();

                await _eventPublisher.PublishAsync(message, cancellationToken);

                message.MarkPublished(DateTimeOffset.UtcNow);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Outbox message publish failed. MessageId: {MessageId}, EventType: {EventType}",
                    message.Id,
                    message.EventType);

                message.MarkFailed();
            }
        }
    }
}
