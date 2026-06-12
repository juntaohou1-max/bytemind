using Logistics.Services.Warehouse.Api.Domain.Inbox;
using Logistics.Services.Warehouse.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Warehouse.Api.Infrastructure.Inbox
{
    /// <summary>
    /// 基于 EF Core 的 Inbox 消息仓储实现。
    /// </summary>
    public class EfCoreInboxMessageRepository : IInboxMessageRepository
    {
        private readonly WarehouseDbContext _dbContext;

        /// <summary>
        /// 创建 EF Core Inbox 消息仓储。
        /// </summary>
        /// <param name="dbContext">Warehouse 数据库上下文。</param>
        public EfCoreInboxMessageRepository(WarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 新增一条 Inbox 记录，标记集成事件已处理。
        /// </summary>
        /// <param name="message">要保存的 Inbox 消息。</param>
        /// <param name="ct">取消令牌。</param>
        public async Task AddAsync(InboxMessage message, CancellationToken ct)
        {
            await _dbContext.Set<InboxMessage>().AddAsync(message, ct);
        }

        /// <summary>
        /// 判断某个集成事件是否已经处理过。
        /// </summary>
        /// <param name="eventId">原始集成事件的唯一标识。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>已处理过则返回 true，否则返回 false。</returns>
        public async Task<bool> ExistsAsync(Guid eventId, CancellationToken ct)
        {
            return await _dbContext.Set<InboxMessage>().AnyAsync(m => m.EventId == eventId, ct);
        }
    }
}
