using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// InventoryReservation 领域实体到 InventoryReservationResult 应用 DTO 的映射器。
    /// </summary>
    public static class InventoryReservationResultMapper
    {
        /// <summary>
        /// 将库存预留领域实体映射为应用层结果。
        /// </summary>
        /// <param name="reservation">库存预留领域实体。</param>
        /// <returns>库存预留应用结果。</returns>
        public static InventoryReservationResult ToResult(this InventoryReservation reservation)
        {
            ArgumentNullException.ThrowIfNull(reservation);

            return new InventoryReservationResult(
                reservation.Id,
                reservation.ExternalOrderNo,
                reservation.SkuId,
                reservation.Quantity,
                reservation.Status.ToString(),
                reservation.CreatedAt);
        }
    }
}
