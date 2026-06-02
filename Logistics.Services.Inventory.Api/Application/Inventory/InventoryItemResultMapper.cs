using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// InventoryItem 领域实体到 InventoryItemResult 应用 DTO 的映射器。
    /// </summary>
    public static class InventoryItemResultMapper
    {
        /// <summary>
        /// 将库存总账领域实体映射为应用层结果。
        /// </summary>
        /// <param name="item">库存总账领域实体。</param>
        /// <returns>库存总账应用结果。</returns>
        public static InventoryItemResult ToResult(this InventoryItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new InventoryItemResult(
                item.Id,
                item.SkuId,
                item.OnHandQuantity,
                item.ReservedQuantity,
                item.DamagedQuantity,
                item.AvailableQuantity);
        }
    }
}
