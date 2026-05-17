namespace Logistics.Services.Inventory.Api.Domain
{
    /// <summary>
    /// 库存流水类型。
    /// </summary>
    public enum InventoryTransactionType
    {
        /// <summary>
        /// 库存调整，例如入库、盘盈或盘亏。
        /// </summary>
        Adjustment = 1,

        /// <summary>
        /// 创建库存预留。
        /// </summary>
        ReservationCreated = 2,

        /// <summary>
        /// 释放库存预留。
        /// </summary>
        ReservationReleased = 3,

        /// <summary>
        /// 扣减库存预留并完成实际出库。
        /// </summary>
        ReservationDeducted = 4
    }
}
