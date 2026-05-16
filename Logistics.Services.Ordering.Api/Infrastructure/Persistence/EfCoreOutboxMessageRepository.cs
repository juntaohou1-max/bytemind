using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    /// <summary>
    /// 基于 EF Core 的 Outbox 消息仓储实现。
    /// </summary>
    public class EfCoreOutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly OrderingDbContext _dbContext;

        public EfCoreOutboxMessageRepository(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(OutboxMessage outboxMessage)
        {
            ArgumentNullException.ThrowIfNull(outboxMessage);

            await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        }

        public async Task<IReadOnlyCollection<OutboxMessage>> GetMessagesByStatusAsync(
            OutboxStatus status,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (take < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(take), "查询数量必须大于 0。");
            }

            // 后台发布器优先处理更早产生的 Pending 消息，并限制批次大小。
            return await _dbContext.OutboxMessages
                .Where(message => message.Status == status)
                .OrderBy(message => message.OccurredAt)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 发布器修改消息状态后，统一通过这里落库。
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
