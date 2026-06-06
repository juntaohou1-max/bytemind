namespace Logistics.Services.Inventory.Api.IntegrationEvents
{
    /// <summary>
    /// 集成事件基类，承载跨服务消息都需要的通用元数据。
    /// </summary>
    public abstract class IntegrationEvent
    {
        /// <summary>
        /// 创建一个新的集成事件，并自动生成事件唯一标识。
        /// </summary>
        /// <param name="eventType">事件类型名称，用于消息路由和反序列化识别。</param>
        /// <param name="tenantId">租户标识，用于区分不同租户的数据。</param>
        /// <param name="aggregateId">聚合根标识，用于说明事件归属的业务对象。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        protected IntegrationEvent(
            string eventType,
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt)
            : this(
                Guid.NewGuid(),
                eventType,
                tenantId,
                aggregateId,
                occurredAt,
                traceId: null,
                version: 1)
        {
        }

        /// <summary>
        /// 根据外部消息中的元数据还原集成事件。
        /// </summary>
        /// <param name="eventId">事件唯一标识，后续可用于 Inbox 幂等判断。</param>
        /// <param name="eventType">事件类型名称，用于消息路由和反序列化识别。</param>
        /// <param name="tenantId">租户标识，用于区分不同租户的数据。</param>
        /// <param name="aggregateId">聚合根标识，用于说明事件归属的业务对象。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <param name="traceId">链路追踪标识，用于串联一次跨服务调用。</param>
        /// <param name="version">事件版本号，用于未来事件结构演进。</param>
        protected IntegrationEvent(
            Guid eventId,
            string eventType,
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string? traceId,
            int version)
        {
            EventId = eventId;
            EventType = eventType;
            TenantId = tenantId;
            AggregateId = aggregateId;
            OccurredAt = occurredAt;
            TraceId = traceId;
            Version = version;
        }

        /// <summary>
        /// 事件唯一标识，后续 Inbox 可以用它判断消息是否已处理。
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// 事件类型名称，例如 OrderCreatedIntegrationEvent。
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// 事件发生时间。
        /// </summary>
        public DateTimeOffset OccurredAt { get; }

        /// <summary>
        /// 链路追踪标识，当前可以为空。
        /// </summary>
        public string? TraceId { get; }

        /// <summary>
        /// 租户标识，用于多租户场景的数据隔离。
        /// </summary>
        public string TenantId { get; }

        /// <summary>
        /// 聚合根标识，用于定位事件归属的业务对象。
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// 事件版本号，默认从 1 开始。
        /// </summary>
        public int Version { get; }
    }
}
