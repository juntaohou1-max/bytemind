using Logistics.Services.Inventory.Api.Application.Inventory;

namespace Logistics.Services.Inventory.Api.Contracts.Inventory
{
    /// <summary>
    /// 库存总账响应。
    /// </summary>
    public class InventoryItemResponse
    {
        /// <summary>
        /// 库存总账 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// SKU 标识。
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 在手库存数量。
        /// </summary>
        public int OnHandQuantity { get; set; }

        /// <summary>
        /// 已锁定库存数量。
        /// </summary>
        public int ReservedQuantity { get; set; }

        /// <summary>
        /// 残损库存数量。
        /// </summary>
        public int DamagedQuantity { get; set; }

        /// <summary>
        /// 可用库存数量。
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// 从应用服务结果创建库存总账响应。
        /// </summary>
        /// <param name="result">库存总账应用服务结果。</param>
        public static InventoryItemResponse FromResult(InventoryItemResult result)
        {
            return new InventoryItemResponse
            {
                Id = result.Id,
                SkuId = result.SkuId,
                OnHandQuantity = result.OnHandQuantity,
                ReservedQuantity = result.ReservedQuantity,
                DamagedQuantity = result.DamagedQuantity,
                AvailableQuantity = result.AvailableQuantity
            };
        }

    }
}
