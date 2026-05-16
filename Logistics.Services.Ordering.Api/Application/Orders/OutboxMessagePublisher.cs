using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistics.Services.Ordering.Api.Application.Orders
{
    /// <summary>
    /// Outbox 后台发布器，定时重试失败消息，并发布待处理消息。
    /// </summary>
    /// <remarks>
    /// 这个后台服务会跟随 API 应用一起启动。它不直接处理 HTTP 请求，而是在后台循环扫描
    /// OutboxMessages 表，把可重试的失败消息重新放回 Pending，然后发布 Pending 消息。
    /// 当前版本还没有接入真正的消息队列，发布动作先用模拟成功来跑通状态流转。
    /// </remarks>
    public class OutboxMessagePublisher : BackgroundService
    {
        // 每轮最多处理的消息数量，避免一次加载和更新过多数据。
        private const int BatchSize = 20;

        // 消息最多失败三次，超过后保持 Failed 状态，等待人工排查。
        private const int MaxRetryCount = 3;

        // 后台发布器轮询间隔，后续可以移动到配置项。
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxMessagePublisher> _logger;

        public OutboxMessagePublisher(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxMessagePublisher> logger)
        {
            // 每轮轮询都创建新的 scope，避免后台服务长期持有同一个 DbContext。
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// BackgroundService 的入口方法，应用启动后由 ASP.NET Core 自动调用。
        /// </summary>
        /// <param name="stoppingToken">应用停止时触发的取消令牌。</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 后台服务会一直循环，直到应用关闭或宿主发出停止信号。
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 先把仍允许重试的 Failed 消息放回 Pending，再统一走发布流程。
                    await RequeueFailedMessagesAsync(stoppingToken);

                    // 再处理所有 Pending 消息，包括刚刚重新入队的失败消息。
                    await PublishPendingMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // 应用正在正常停止时，不把取消当成错误记录。
                    return;
                }
                catch (Exception exception)
                {
                    // 单轮扫描失败不能让后台服务退出，记录日志后等待下一轮继续尝试。
                    _logger.LogError(exception, "Outbox publisher execution failed.");
                }

                // 控制轮询频率，避免后台任务一直空转访问数据库。
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        /// <summary>
        /// 将还没超过重试上限的 Failed 消息重新放回 Pending。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task RequeueFailedMessagesAsync(CancellationToken cancellationToken)
        {
            // BackgroundService 是单例生命周期；这里临时创建 scope 来获取 scoped 仓储和 DbContext。
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
                // 仓储已经按 RetryCount 过滤一次，这里再走领域方法，保证业务规则集中在领域对象里。
                if (!message.CanRetry(MaxRetryCount))
                {
                    continue;
                }

                // 重新进入 Pending 后，下一步会被 PublishPendingMessagesAsync 正常发布。
                message.MarkPendingForRetry();
                requeuedCount++;
            }

            if (requeuedCount > 0)
            {
                // 只有状态真的发生变化时才提交数据库。
                await outboxMessageRepository.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 查询并发布一批 Pending 消息。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            // 每批处理使用独立 DbContext，避免后台服务长期跟踪大量实体。
            using var scope = _scopeFactory.CreateScope();

            var outboxMessageRepository = scope.ServiceProvider
                .GetRequiredService<IOutboxMessageRepository>();

            var messages = await outboxMessageRepository.GetMessagesByStatusAsync(
                OutboxStatus.Pending,
                BatchSize,
                cancellationToken);

            foreach (var message in messages)
            {
                // 单条消息内部负责 Publishing、Published、Failed 状态流转。
                await PublishMessageAsync(message, cancellationToken);
            }

            if (messages.Count > 0)
            {
                // 一批消息统一提交，减少数据库往返次数。
                await outboxMessageRepository.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 发布单条 Outbox 消息，并根据结果更新消息状态。
        /// </summary>
        /// <param name="message">要发布的 Outbox 消息。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        private async Task PublishMessageAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            try
            {
                // 标记为 Publishing，表示后台发布器已经领取并正在处理这条消息。
                message.MarkPublishing();

                await PublishToMessageBrokerAsync(message, cancellationToken);

                // 发布成功后记录完成时间，后台发布器后续不会再处理 Published 消息。
                message.MarkPublished(DateTimeOffset.UtcNow);
            }
            catch (OperationCanceledException)
            {
                // 应用停止时直接向上抛出，让 ExecuteAsync 按正常停止处理。
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Outbox message publish failed. MessageId: {MessageId}, EventType: {EventType}",
                    message.Id,
                    message.EventType);

                // 发布失败时累计失败次数；达到上限后会保持 Failed，等待人工排查。
                message.MarkFailed();
            }
        }

        /// <summary>
        /// 真正发布到消息系统的位置。
        /// </summary>
        /// <remarks>
        /// 当前阶段还没有接 RabbitMQ、Kafka 或 HTTP 回调，所以这里先模拟成功。
        /// 将来接入真实消息队列时，可以把发送逻辑替换到这个方法里。
        /// </remarks>
        /// <param name="message">要发布的 Outbox 消息。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        private static Task PublishToMessageBrokerAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            // 第一版先模拟发布成功，后续可以在这里接入 RabbitMQ、Kafka 或 HTTP 回调。
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
