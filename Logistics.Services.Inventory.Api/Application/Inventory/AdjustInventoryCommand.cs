namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// 调整库存命令，表示一次库存增减操作的输入数据。
    /// </summary>
    /// <param name="SkuId">要调整库存的 SKU 标识。</param>
    /// <param name="QuantityDelta">库存增减变化量，正数表示增加，负数表示减少。</param>
    /// <param name="ReferenceNo">外部参考号，例如盘点单号或入库单号。</param>
    public record AdjustInventoryCommand(
        string SkuId,
        int QuantityDelta,
        string? ReferenceNo = null);
}
