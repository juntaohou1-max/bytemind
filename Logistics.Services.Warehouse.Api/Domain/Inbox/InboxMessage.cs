namespace Logistics.Services.Warehouse.Api.Domain.Inbox
{
    /// <summary>
    /// Inbox 消息，用于记录已处理的集成事件，保证重复投递时能幂等跳过。
    /// </summary>
    /// <remarks>
    /// 消费端收到集成事件后，先根据 EventId 查询 Inbox 是否已有记录。
    /// 有记录说明已处理过，直接跳过；没有则处理业务逻辑，并在同一事务中写入 Inbox 记录。
    /// </remarks>
    public class InboxMessage
    {
        /// <summary>
        /// Inbox 记录在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 原始集成事件的唯一标识，用于幂等判断。同一 EventId 只会被处理一次。
        /// </summary>
        public Guid EventId { get; private set; }

        /// <summary>
        /// 集成事件类型名称，例如 OrderCreatedIntegrationEvent。
        /// </summary>
        public string EventType { get; private set; } = string.Empty;

        /// <summary>
        /// 租户标识，用于多租户场景的数据隔离。
        /// </summary>
        public string TenantId { get; private set; } = string.Empty;

        /// <summary>
        /// 原始集成事件的 JSON 载荷，用于审计和问题排查。
        /// </summary>
        public string Payload { get; private set; } = string.Empty;

        /// <summary>
        /// 事件处理完成时间。
        /// </summary>
        public DateTimeOffset ProcessedAt { get; private set; }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private InboxMessage()
        {
        }

        /// <summary>
        /// 创建一条 Inbox 记录，标记某个集成事件已经处理完成。
        /// </summary>
        /// <param name="eventId">原始集成事件的唯一标识。</param>
        /// <param name="eventType">集成事件类型名称。</param>
        /// <param name="payload">原始集成事件的 JSON 载荷。</param>
        /// <param name="tenantId">租户标识。</param>
        public InboxMessage(Guid eventId, string eventType, string payload, string tenantId)
        {
            if (eventId == Guid.Empty)
            {
                throw new ArgumentException("事件标识不能为空 Guid。", nameof(eventId));
            }

            EventId = eventId;
            EventType = EnsureRequired(eventType, nameof(eventType));
            Payload = EnsureRequired(payload, nameof(payload));
            TenantId = EnsureRequired(tenantId, nameof(tenantId));
            ProcessedAt = DateTimeOffset.UtcNow;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// 校验必填字符串字段不能为空。
        /// </summary>
        /// <param name="value">要校验的字符串值。</param>
        /// <param name="parameterName">参数名称，用于异常消息。</param>
        private static string EnsureRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Inbox 消息必填字段不能为空。", parameterName);
            }

            return value.Trim();
        }
    }
}
