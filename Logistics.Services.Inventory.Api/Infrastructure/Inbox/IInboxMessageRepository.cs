using Logistics.Services.Inventory.Api.Domain.Inbox;

namespace Logistics.Services.Inventory.Api.Infrastructure.Inbox
{
    /// <summary>
    /// Inbox 消息仓储接口，用于封装 InboxMessage 的持久化和幂等查询操作。
    /// </summary>
    public interface IInboxMessageRepository
    {
        /// <summary>
        /// 判断某个集成事件是否已经处理过。
        /// </summary>
        /// <param name="eventId">原始集成事件的唯一标识。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>已处理过则返回 true，否则返回 false。</returns>
        Task<bool> ExistsAsync(Guid eventId, CancellationToken ct);

        /// <summary>
        /// 新增一条 Inbox 记录，标记集成事件已处理。
        /// </summary>
        /// <param name="message">要保存的 Inbox 消息。</param>
        /// <param name="ct">取消令牌。</param>
        Task AddAsync(InboxMessage message, CancellationToken ct);
    }
}
