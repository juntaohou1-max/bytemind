using Logistics.Services.Warehouse.Api.Domain.Outbox;
using Logistics.Services.Warehouse.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Warehouse.Api.Infrastructure.Outbox
{
    /// <summary>
    /// 基于 EF Core 的 Outbox 消息仓储实现。
    /// </summary>
    public class EfCoreOutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly WarehouseDbContext _dbContext;

        /// <summary>
        /// 创建 EF Core Outbox 消息仓储。
        /// </summary>
        /// <param name="dbContext">Warehouse 数据库上下文。</param>
        public EfCoreOutboxMessageRepository(WarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 新增一条 Outbox 消息。
        /// </summary>
        /// <param name="outboxMessage">待保存的 Outbox 消息。</param>
        public async Task AddAsync(OutboxMessage outboxMessage)
        {
            ArgumentNullException.ThrowIfNull(outboxMessage);

            await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
        }

        /// <summary>
        /// 按状态查询一批 Outbox 消息，优先处理更早产生的消息。
        /// </summary>
        /// <param name="status">要查询的消息状态。</param>
        /// <param name="take">本次最多取多少条。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>符合状态的消息集合。</returns>
        public async Task<IReadOnlyCollection<OutboxMessage>> GetMessagesByStatusAsync(
            OutboxStatus status,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (take < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(take), "查询数量必须大于 0。");
            }

            return await _dbContext.Set<OutboxMessage>()
                .Where(message => message.Status == status)
                .OrderBy(message => message.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询还没有超过最大重试次数的失败消息。
        /// </summary>
        /// <param name="maxRetryCount">最大失败重试次数。</param>
        /// <param name="take">本次最多取多少条。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>可以重新放回待发布队列的失败消息集合。</returns>
        public async Task<IReadOnlyCollection<OutboxMessage>> GetFailedMessagesForRetryAsync(
            int maxRetryCount,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (maxRetryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount), "最大重试次数不能小于 0。");
            }

            if (take < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(take), "查询数量必须大于 0。");
            }

            return await _dbContext.Set<OutboxMessage>()
                .Where(message =>
                    message.Status == OutboxStatus.Failed &&
                    message.RetryCount < maxRetryCount)
                .OrderBy(message => message.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 保存 Outbox 消息状态变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
