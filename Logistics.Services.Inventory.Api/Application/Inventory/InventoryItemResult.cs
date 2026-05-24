namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// 库存总账结果，用于应用服务向接口层返回库存当前状态。
    /// </summary>
    /// <param name="Id">库存总账 ID。</param>
    /// <param name="SkuId">SKU 标识。</param>
    /// <param name="OnHandQuantity">在手库存数量。</param>
    /// <param name="ReservedQuantity">已锁定库存数量。</param>
    /// <param name="DamagedQuantity">残损库存数量。</param>
    /// <param name="AvailableQuantity">可用库存数量。</param>
    public record InventoryItemResult(
        Guid Id,
        string SkuId,
        int OnHandQuantity,
        int ReservedQuantity,
        int DamagedQuantity,
        int AvailableQuantity);
}
