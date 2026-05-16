using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    /// <summary>
    /// Outbox 后台发布器，定时扫描 Pending 消息并推进发布状态。
    /// </summary>
    /// <remarks>
    /// 当前版本先模拟发布成功，用于跑通 Pending -> Publishing -> Published 的完整流程。
    /// 后续接入消息队列时，只需要替换 PublishToMessageBrokerAsync 内部实现。
    /// </remarks>
    public class OutboxMessagePublisher : BackgroundService
    {
        // 每轮最多处理的消息数量，避免一次性加载和更新过多数据。
        private const int BatchSize = 20;

        // 后台发布器的轮询间隔。后续可以改成配置项。
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        public OutboxMessagePublisher(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxMessagePublisher> logger)
        {
            // Create a fresh scope for each polling cycle so DbContext is not held for the service lifetime.
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // ExecuteAsync 是 BackgroundService 的入口，应用启动后会持续运行到应用停止。
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingMessagesAsync(stoppingToken);
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

        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            // BackgroundService 是长生命周期对象；每轮创建 scope，避免长期持有 DbContext。
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

            // 同一批消息的状态变更统一保存，减少数据库提交次数。
            await outboxMessageRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task PublishMessageAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            try
            {
                // 先标记为发布中，表示后台发布器已经领取这条消息。
                message.MarkPublishing();

                await PublishToMessageBrokerAsync(message, cancellationToken);//发布消息

                // 模拟发布成功后标记为已发布，并记录处理完成时间。
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

        private static Task PublishToMessageBrokerAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            // 如果应用正在停止，就立刻抛出取消异常，不继续处理这条消息
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
