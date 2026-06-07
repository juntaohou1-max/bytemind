namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 仓库状态，表示仓库当前是否允许业务操作。
    /// </summary>
    public enum WarehouseStatus
    {
        /// <summary>
        /// 运营中：仓库正常运作，可以处理收发存业务。
        /// </summary>
        Active = 1,

        /// <summary>
        /// 已停用：仓库已关闭，不再接受任何业务操作。
        /// </summary>
        Inactive = 2
    }
}
