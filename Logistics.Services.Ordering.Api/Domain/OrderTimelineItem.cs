namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// 订单时间线记录。
    /// </summary>
    /// <remarks>
    /// 用于记录订单生命周期中发生过的关键事件，例如创建、取消等。
    /// </remarks>
    public class OrderTimelineItem
    {
        /// <summary>
        /// 时间线事件类型。
        /// </summary>
        public string EventType { get; private set; } = string.Empty;

        /// <summary>
        /// 事件说明。
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// 事件发生时间。
        /// </summary>
        public DateTimeOffset OccurredAt { get; private set; }

        /// <summary>
        /// 创建订单时间线记录。
        /// </summary>
        /// <param name="eventType">时间线事件类型。</param>
        /// <param name="description">事件说明。</param>
        /// <param name="occurredAt">事件发生时间。</param>
        /// <exception cref="ArgumentException">事件类型或事件说明为空时抛出。</exception>
        public OrderTimelineItem(
            string eventType,
            string description,
            DateTimeOffset occurredAt)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("事件类型不能为空。", nameof(eventType));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("事件说明不能为空。", nameof(description));
            }

            EventType = eventType.Trim();
            Description = description.Trim();
            OccurredAt = occurredAt;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private OrderTimelineItem()
        {
        }
    }
}
