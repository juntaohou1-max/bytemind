namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// Outbox 消息发布状态。
    /// </summary>
    public enum OutboxStatus
    {
        /// <summary>
        /// 待发布，后台发布器会优先扫描这个状态的消息。
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 发布中，表示消息已经被后台发布器领取。
        /// </summary>
        Publishing = 1,

        /// <summary>
        /// 已发布，表示消息已经成功发送到外部消息系统或目标服务。
        /// </summary>
        Published = 2,

        /// <summary>
        /// 发布失败，后续可以根据重试策略重新发布或人工排查。
        /// </summary>
        Failed = 3
    }
}
