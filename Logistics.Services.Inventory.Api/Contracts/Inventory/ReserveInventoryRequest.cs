namespace Logistics.Services.Inventory.Api.Contracts.Inventory
{
    /// <summary>
    /// 锁定库存请求。
    /// </summary>
    public class ReserveInventoryRequest
    {
        /// <summary>
        /// SKU 标识。
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 外部系统传入的订单号。
        /// </summary>
        public string ExternalOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 本次需要锁定的库存数量。
        /// </summary>
        public int Quantity { get; set; }
    }
}
