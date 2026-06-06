using Logistics.Services.Inventory.Api.Domain.Outbox;

namespace Logistics.Services.Inventory.Api.Application.IntegrationEvents
{
    /// <summary>
    /// 集成事件发布器接口，负责将 Outbox 消息发往外部消息系统或目标服务。
    /// </summary>
    /// <remarks>
    /// 当前开发阶段使用控制台日志实现，后续可以替换为 RabbitMQ、Kafka 等真实发布器。
    /// </remarks>
    public interface IIntegrationEventPublisher
    {
        /// <summary>
        /// 发布一条集成事件。
        /// </summary>
        /// <param name="message">待发布的 Outbox 消息。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
    }
}
