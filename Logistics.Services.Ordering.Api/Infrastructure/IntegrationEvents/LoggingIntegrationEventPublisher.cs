using Logistics.Services.Ordering.Api.Application.IntegrationEvents;
using Logistics.Services.Ordering.Api.Domain;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Ordering.Api.Infrastructure.IntegrationEvents
{
    /// <summary>
    /// Development publisher that logs integration events instead of sending them to a real broker.
    /// </summary>
    public class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

        public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Integration event published. MessageId: {MessageId}, EventType: {EventType}",
                message.Id,
                message.EventType);

            return Task.CompletedTask;
        }
    }
}
