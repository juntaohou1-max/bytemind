using Logistics.Services.Inventory.Api.Application.IntegrationEvents;
using Logistics.Services.Inventory.Api.Domain.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Inventory.Api.Infrastructure.Outbox
{
    /// <summary>
    /// Outbox 后台发布器，应用启动后自动扫描并处理待发布和可重试的 Outbox 消息。
    /// </summary>
    /// <remarks>
    /// 发布器负责调度和状态转换，实际消息投递委托给 IIntegrationEventPublisher，
    /// 后续可替换为 RabbitMQ、Kafka 或 HTTP 投递实现。
    /// </remarks>
    public class OutboxMessagePublisher : BackgroundService
    {
        /// <summary>
        /// 单次轮询最多处理的消息数量，避免一次加载过多数据。
        /// </summary>
        private const int BatchSize = 20;

        /// <summary>
        /// 失败消息超过此重试次数后停止自动重试，等待人工处理。
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// 轮询间隔，后续可移至配置。
        /// </summary>
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        /// <summary>
        /// 创建 Outbox 后台发布器。
        /// </summary>
        /// <param name="scopeFactory">服务作用域工厂，用于在后台线程中创建数据库作用域。</param>
        /// <param name="eventPublisher">集成事件发布器。</param>
        /// <param name="logger">日志记录器。</param>
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
        /// 执行一次完整的 Outbox 处理轮次。测试可直接调用此方法而不启动后台循环。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task ProcessOnceAsync(CancellationToken cancellationToken)
        {
            await RequeueFailedMessagesAsync(cancellationToken);
            await PublishPendingMessagesAsync(cancellationToken);
        }

        /// <summary>
        /// ASP.NET Core 启动时调用的后台服务入口。
        /// </summary>
        /// <param name="stoppingToken">应用停止时触发的取消令牌。</param>
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
                    _logger.LogError(exception, "Outbox 发布器执行失败。");
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        /// <summary>
        /// 将可重试的失败消息重新放回待发布队列。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
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
        /// 加载并发布一批待发布消息。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
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
        /// 发布单条消息并根据结果更新状态。
        /// </summary>
        /// <param name="message">待发布的 Outbox 消息。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
                    "Outbox 消息发布失败。MessageId: {MessageId}, EventType: {EventType}",
                    message.Id,
                    message.EventType);

                message.MarkFailed();
            }
        }
    }
}
