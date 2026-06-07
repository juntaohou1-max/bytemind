namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 货位状态，表示当前货位的可用情况。
    /// </summary>
    public enum BinStatus
    {
        /// <summary>
        /// 可用：货位为空，可以存放商品。
        /// </summary>
        Available = 1,

        /// <summary>
        /// 已占用：货位中已存放商品。
        /// </summary>
        Occupied = 2,

        /// <summary>
        /// 已预留：货位已被某个出库任务预留。
        /// </summary>
        Reserved = 3,

        /// <summary>
        /// 已锁定：货位因盘点、维修等原因被锁定，不可使用。
        /// </summary>
        Blocked = 4
    }
}
