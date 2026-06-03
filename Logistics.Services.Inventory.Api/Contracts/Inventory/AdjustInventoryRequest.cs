namespace Logistics.Services.Inventory.Api.Contracts.Inventory
{
    /// <summary>
    /// 调整库存请求。
    /// </summary>
    public class AdjustInventoryRequest
    {
        /// <summary>
        /// SKU 标识。
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 库存增减变化量，正数表示增加，负数表示减少。
        /// </summary>
        public int QuantityDelta { get; set; }

        /// <summary>
        /// 外部参考号，例如盘点单号或入库单号。
        /// </summary>
        public string? ReferenceNo { get; set; }
    }
}
