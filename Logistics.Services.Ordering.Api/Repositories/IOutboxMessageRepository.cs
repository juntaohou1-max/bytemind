using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    /// <summary>
    /// Outbox 消息仓储，负责保存待发布消息，并给后台发布器提供待处理消息。
    /// </summary>
    public interface IOutboxMessageRepository
    {
        /// <summary>
        /// 新增一条 Outbox 消息。通常和业务数据在同一次 SaveChangesAsync 中提交。
        /// </summary>
        /// <param name="outboxMessage">待保存的 Outbox 消息。</param>
        Task AddAsync(OutboxMessage outboxMessage);

        /// <summary>
        /// 按状态查询一批 Outbox 消息。
        /// </summary>
        /// <param name="status">要查询的消息状态。</param>
        /// <param name="take">本次最多取多少条，避免后台任务一次加载过多数据。</param>
        /// <param name="cancellationToken">取消令牌，用于应用停止时及时中断数据库查询。</param>
        /// <returns>符合状态的消息集合。</returns>
        Task<IReadOnlyCollection<OutboxMessage>> GetMessagesByStatusAsync(
            OutboxStatus status,
            int take,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询还没有超过最大重试次数的失败消息。
        /// </summary>
        /// <param name="maxRetryCount">最大失败重试次数。</param>
        /// <param name="take">本次最多取多少条。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>可以重新放回待发布队列的失败消息集合。</returns>
        Task<IReadOnlyCollection<OutboxMessage>> GetFailedMessagesForRetryAsync(
            int maxRetryCount,
            int take,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存 Outbox 消息状态变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
