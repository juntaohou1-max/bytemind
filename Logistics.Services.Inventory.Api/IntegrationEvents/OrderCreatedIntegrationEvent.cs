using System.Text.Json.Serialization;

namespace Logistics.Services.Inventory.Api.IntegrationEvents
{
    /// <summary>
    /// 订单已创建事件，Inventory 后续会根据它尝试为订单明细锁定库存。
    /// </summary>
    public class OrderCreatedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 订单已创建事件的事件类型名称。
        /// </summary>
        public const string TypeName = "OrderCreatedIntegrationEvent";

        /// <summary>
        /// 创建一个新的订单已创建事件。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="aggregateId">订单聚合根标识。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <param name="externalOrderNo">外部订单号。</param>
        /// <param name="customerId">客户标识。</param>
        /// <param name="lines">订单明细行，包含需要锁定库存的 SKU 和数量。</param>
        public OrderCreatedIntegrationEvent(
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string externalOrderNo,
            string customerId,
            IReadOnlyCollection<OrderCreatedIntegrationEventLine> lines)
            : this(
                Guid.NewGuid(),
                tenantId,
                aggregateId,
                occurredAt,
                null,
                1,
                externalOrderNo,
                customerId,
                lines)
        {
        }

        /// <summary>
        /// 从消息载荷中还原订单已创建事件。
        /// </summary>
        /// <param name="eventId">事件唯一标识，后续可用于 Inbox 幂等判断。</param>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="aggregateId">订单聚合根标识。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <param name="traceId">链路追踪标识。</param>
        /// <param name="version">事件版本号。</param>
        /// <param name="externalOrderNo">外部订单号。</param>
        /// <param name="customerId">客户标识。</param>
        /// <param name="lines">订单明细行，包含需要锁定库存的 SKU 和数量。</param>
        [JsonConstructor]
        public OrderCreatedIntegrationEvent(
            Guid eventId,
            string tenantId,
            string aggregateId,
            DateTimeOffset occurredAt,
            string? traceId,
            int version,
            string externalOrderNo,
            string customerId,
            IReadOnlyCollection<OrderCreatedIntegrationEventLine> lines)
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
            CustomerId = customerId;
            Lines = lines;
        }

        /// <summary>
        /// 外部订单号，用于和 Ordering 侧订单建立关联。
        /// </summary>
        public string ExternalOrderNo { get; }

        /// <summary>
        /// 客户标识，当前只作为事件上下文保留。
        /// </summary>
        public string CustomerId { get; }

        /// <summary>
        /// 订单明细行，后续库存锁定会逐行读取 SKU 和数量。
        /// </summary>
        public IReadOnlyCollection<OrderCreatedIntegrationEventLine> Lines { get; }
    }

    /// <summary>
    /// 订单已创建事件中的单个订单明细行。
    /// </summary>
    public class OrderCreatedIntegrationEventLine
    {
        /// <summary>
        /// 创建一个订单已创建事件明细行。
        /// </summary>
        /// <param name="skuId">需要锁定库存的 SKU 标识。</param>
        /// <param name="quantity">需要锁定的库存数量。</param>
        [JsonConstructor]
        public OrderCreatedIntegrationEventLine(
            string skuId,
            int quantity)
        {
            SkuId = skuId;
            Quantity = quantity;
        }

        /// <summary>
        /// 需要锁定库存的 SKU 标识。
        /// </summary>
        public string SkuId { get; }

        /// <summary>
        /// 需要锁定的库存数量。
        /// </summary>
        public int Quantity { get; }
    }
}
