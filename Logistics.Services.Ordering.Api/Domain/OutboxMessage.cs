namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// Outbox 消息，用于把“业务数据保存”和“集成事件发布”拆成两个可靠步骤。
    /// </summary>
    /// <remarks>
    /// 创建或变更订单时，可以在同一个数据库事务中写入业务表和 OutboxMessages 表。
    /// 后续由后台任务扫描待发布消息，再把消息发送给消息队列或其他服务。
    /// </remarks>
    public class OutboxMessage
    {
        /// <summary>
        /// Outbox 消息在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 集成事件类型，例如 OrderCreated 或 OrderCancelled。
        /// </summary>
        public string EventType { get; private set; } = string.Empty;

        /// <summary>
        /// 事件内容，通常保存为 JSON 字符串。
        /// </summary>
        public string Payload { get; private set; } = string.Empty;

        /// <summary>
        /// 消息当前发布状态。
        /// </summary>
        public OutboxStatus Status { get; private set; }

        /// <summary>
        /// 业务事件发生时间。
        /// </summary>
        public DateTimeOffset OccurredAt { get; private set; }

        /// <summary>
        /// 消息成功发布完成时间；未发布成功前为空。
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; private set; }

        /// <summary>
        /// 发布失败次数，用于后续做重试和告警。
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// 创建一条待发布的 Outbox 消息。
        /// </summary>
        /// <param name="eventType">集成事件类型。</param>
        /// <param name="payload">事件内容，通常是 JSON。</param>
        public OutboxMessage(string eventType, string payload)
            : this(eventType, payload, DateTimeOffset.UtcNow)
        {
        }

        /// <summary>
        /// 创建一条待发布的 Outbox 消息，并显式指定事件发生时间。
        /// </summary>
        /// <param name="eventType">集成事件类型。</param>
        /// <param name="payload">事件内容，通常是 JSON。</param>
        /// <param name="occurredAt">业务事件发生时间。</param>
        public OutboxMessage(
            string eventType,
            string payload,
            DateTimeOffset occurredAt)
        {
            Id = Guid.NewGuid();
            EventType = EnsureRequired(eventType, nameof(eventType));
            Payload = EnsureRequired(payload, nameof(payload));
            Status = OutboxStatus.Pending;
            OccurredAt = occurredAt;
        }

        /// <summary>
        /// 保留给 EF Core 使用的无参构造函数。
        /// </summary>
        private OutboxMessage()
        {
        }

        /// <summary>
        /// 标记消息正在发布。
        /// </summary>
        public void MarkPublishing()
        {
            if (Status == OutboxStatus.Published)
            {
                throw new InvalidOperationException("已发布的 Outbox 消息不能重新进入发布中状态。");
            }

            Status = OutboxStatus.Publishing;
        }

        /// <summary>
        /// 标记消息已经发布成功。
        /// </summary>
        /// <param name="processedAt">发布完成时间。</param>
        public void MarkPublished(DateTimeOffset processedAt)
        {
            Status = OutboxStatus.Published;
            ProcessedAt = processedAt;
        }

        /// <summary>
        /// 标记消息发布失败，并累计一次重试次数。
        /// </summary>
        public void MarkFailed()
        {
            if (Status == OutboxStatus.Published)
            {
                throw new InvalidOperationException("已发布的 Outbox 消息不能再标记为失败。");
            }

            Status = OutboxStatus.Failed;
            RetryCount++;
        }

        private static string EnsureRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Outbox 消息必填字段不能为空。", parameterName);
            }

            return value.Trim();
        }
    }
}
