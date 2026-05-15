namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// 订单状态。
    /// </summary>
    /// <remarks>
    /// 第一阶段只表达接单和早期履约相关的基础状态。
    /// 后续加入库存、拣货、出库、运输后，可以继续扩展更细的履约状态。
    /// </remarks>
    public enum OrderStatus
    {
        /// <summary>
        /// 待校验。
        /// </summary>
        /// <remarks>
        /// 可用于后续扩展异步校验流程，例如校验客户、仓库、SKU 或重复单。
        /// 当前第一版构造成功后会直接进入 <see cref="Created"/>。
        /// </remarks>
        PendingValidation = 0,

        /// <summary>
        /// 已创建。
        /// </summary>
        /// <remarks>
        /// 表示订单已经通过当前阶段的基础规则，可以被保存和查询。
        /// </remarks>
        Created = 1,

        /// <summary>
        /// 库存已锁定。
        /// </summary>
        /// <remarks>
        /// 表示库存服务已经为该订单完成库存预留。
        /// </remarks>
        InventoryReserved = 2,

        /// <summary>
        /// 履约单已创建。
        /// </summary>
        /// <remarks>
        /// 表示订单已经生成对应的履约单，可以进入仓储履约流程。
        /// </remarks>
        FulfillmentCreated = 3,

        /// <summary>
        /// 已取消。
        /// </summary>
        /// <remarks>
        /// 表示订单不再继续履约。
        /// 第一阶段尚未引入“已发货”状态，所以取消规则暂时保持简单。
        /// </remarks>
        Cancelled = 4
    }
}
