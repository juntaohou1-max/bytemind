namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// Inventory 应用服务接口，用于承载接口层需要调用的库存业务用例。
    /// </summary>
    public interface IInventoryApplicationService
    {
        /// <summary>
        /// 根据 SKU 查询库存总账。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账结果，找不到则返回 null。</returns>
        Task<InventoryItemResult?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 调整库存。
        /// </summary>
        /// <param name="command">调整库存命令。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>调整后的库存总账结果。</returns>
        Task<InventoryItemResult> AdjustInventoryAsync(AdjustInventoryCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// 锁定库存。
        /// </summary>
        /// <param name="command">锁定库存命令。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>库存预留结果。</returns>
        Task<InventoryReservationResult> ReserveInventoryAsync(ReserveInventoryCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// 释放库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 扣减库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task DeductReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    }
}
