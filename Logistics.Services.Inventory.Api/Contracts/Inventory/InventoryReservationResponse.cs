using Logistics.Services.Inventory.Api.Application.Inventory;

namespace Logistics.Services.Inventory.Api.Contracts.Inventory
{
    /// <summary>
    /// 库存预留响应。
    /// </summary>
    public class InventoryReservationResponse
    {
        /// <summary>
        /// 库存预留 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 外部系统传入的订单号。
        /// </summary>
        public string ExternalOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// SKU 标识。
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 本次锁定的库存数量。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 库存预留状态。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 库存预留创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 从应用服务结果创建库存预留响应。
        /// </summary>
        /// <param name="result">库存预留应用服务结果。</param>
        public static InventoryReservationResponse FromResult(InventoryReservationResult result)
        {
            return new InventoryReservationResponse
            {
                Id = result.Id,
                ExternalOrderNo = result.ExternalOrderNo,
                SkuId = result.SkuId,
                Quantity = result.Quantity,
                Status = result.Status,
                CreatedAt = result.CreatedAt
            };
        }
    }
}
