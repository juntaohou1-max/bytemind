using System.Text.Json.Serialization;

namespace Logistics.Services.Inventory.Api.IntegrationEvents
{
    /// <summary>
    /// 库存已锁定事件，表示 Inventory 已经完成某个订单的库存预留。
    /// </summary>
    public class InventoryReservedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 库存已锁定事件的事件类型名称。
        /// </summary>
        public const string TypeName = "InventoryReservedIntegrationEvent";

        /// <summary>
        /// 创建一个新的库存已锁定事件。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="aggregateId">事件归属的聚合根标识。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <param name="externalOrderNo">外部订单号。</param>
        /// <param name="status">库存锁定后的状态文本。</param>
        public InventoryReservedIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string status)
            : this(
                Guid.NewGuid(),
                tenantId,
                aggregateId,
                occurredAt,
                null,
                1,
                externalOrderNo,
                status)
        {
        }

        /// <summary>
        /// 从消息载荷中还原库存已锁定事件。
        /// </summary>
        /// <param name="eventId">事件唯一标识，后续可用于 Inbox 或 Outbox 幂等判断。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="aggregateId">事件归属的聚合根标识。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <param name="traceId">链路追踪标识。</param>
        /// <param name="version">事件版本号。</param>
        /// <param name="externalOrderNo">外部订单号。</param>
        /// <param name="status">库存锁定后的状态文本。</param>
        [JsonConstructor]
        public InventoryReservedIntegrationEvent(
            Guid eventId,
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string? traceId,
            int version,
            string externalOrderNo,
            string status)
            : base(
                eventId,
                TypeName,
                tenantId,
                aggregateId,
                occurredAt,
                traceId,
                version)
        {
            ExternalOrderNo = externalOrderNo;
            Status = status;
        }

        /// <summary>
        /// 外部订单号，用于通知 Ordering 或 Fulfillment 当前订单的库存状态。
        /// </summary>
        public string ExternalOrderNo { get; }

        /// <summary>
        /// 库存锁定后的状态文本，当前先保留为简单字符串。
        /// </summary>
        public string Status { get; }
    }
}
