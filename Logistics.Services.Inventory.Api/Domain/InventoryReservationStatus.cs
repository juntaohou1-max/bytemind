namespace Logistics.Services.Inventory.Api.Domain
{
    /// <summary>
    /// 库存预留状态。
    /// </summary>
    public enum InventoryReservationStatus
    {
        /// <summary>
        /// 库存已经锁定，尚未释放或扣减。
        /// </summary>
        Active = 1,

        /// <summary>
        /// 库存预留已经释放。
        /// </summary>
        Released = 2,

        /// <summary>
        /// 库存预留已经扣减并完成实际出库。
        /// </summary>
        Deducted = 3
    }
}
