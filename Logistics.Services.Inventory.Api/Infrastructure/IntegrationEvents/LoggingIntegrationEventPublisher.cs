using Logistics.Services.Inventory.Api.Application.IntegrationEvents;
using Logistics.Services.Inventory.Api.Domain.Outbox;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Inventory.Api.Infrastructure.IntegrationEvents
{
    /// <summary>
    /// 开发阶段集成事件发布器，当前只记录日志，后续替换为 RabbitMQ / Kafka 等真实发布器。
    /// </summary>
    public class LoggingIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly ILogger<LoggingIntegrationEventPublisher> _logger;

        /// <summary>
        /// 创建日志集成事件发布器。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        public LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 以日志形式发布集成事件。
        /// </summary>
        /// <param name="message">待发布的 Outbox 消息。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
