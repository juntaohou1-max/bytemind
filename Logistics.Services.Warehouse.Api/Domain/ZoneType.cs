namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 仓储区域类型，表示仓库内部区域的用途分类。
    /// </summary>
    public enum ZoneType
    {
        /// <summary>
        /// 拣货区：用于订单拣选操作的区域。
        /// </summary>
        Picking = 1,

        /// <summary>
        /// 存储区：用于大批量存储商品的区域。
        /// </summary>
        Storage = 2,

        /// <summary>
        /// 收货区：用于接收和质检新到货物的区域。
        /// </summary>
        Receiving = 3,

        /// <summary>
        /// 退货区：用于处理客户退回商品的区域。
        /// </summary>
        Returns = 4
    }
}
