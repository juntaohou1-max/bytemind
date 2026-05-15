namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class OrderTimelineItemResponse
    {
        /// <summary>
        /// 时间线事件类型。
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// 事件说明。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 事件发生时间。
        /// </summary>
        public DateTimeOffset OccurredAt { get; set; }
    }
}
