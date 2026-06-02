using Logistics.Services.Inventory.Api.Repositories;

namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// Inventory 应用服务，负责组织库存用例流程并调用领域模型和仓储。
    /// </summary>
    public class InventoryApplicationService : IInventoryApplicationService
    {
        private readonly IInventoryItemRepository _inventoryItemRepository;

        /// <summary>
        /// 创建 Inventory 应用服务。
        /// </summary>
        /// <param name="inventoryItemRepository">库存总账仓储。</param>
        public InventoryApplicationService(IInventoryItemRepository inventoryItemRepository)
        {
            _inventoryItemRepository = inventoryItemRepository;
        }

        /// <summary>
        /// 根据 SKU 查询库存总账。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账结果，找不到则返回 null。</returns>
        public async Task<InventoryItemResult?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default)
        {
            EnsureSkuId(skuId);

            var item = await _inventoryItemRepository.GetBySkuIdAsync(skuId, cancellationToken);

            return item?.ToResult();
        }

        /// <summary>
        /// 调整库存。
        /// </summary>
        /// <param name="command">调整库存命令。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>调整后的库存总账结果。</returns>
        /// <exception cref="KeyNotFoundException">当指定 SKU 的库存总账不存在时抛出。</exception>
        public async Task<InventoryItemResult> AdjustInventoryAsync(AdjustInventoryCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            EnsureSkuId(command.SkuId);

            var item = await _inventoryItemRepository.GetBySkuIdAsync(command.SkuId, cancellationToken)
                ?? throw new KeyNotFoundException($"SKU '{command.SkuId}' 的库存总账不存在，无法调整。");

            item.Adjust(command.QuantityDelta, command.ReferenceNo);

            await _inventoryItemRepository.SaveChangesAsync(cancellationToken);

            return item.ToResult();
        }

        /// <summary>
        /// 锁定库存。
        /// </summary>
        /// <param name="command">锁定库存命令。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>库存预留结果。</returns>
        /// <exception cref="KeyNotFoundException">当指定 SKU 的库存总账不存在时抛出。</exception>
        public async Task<InventoryReservationResult> ReserveInventoryAsync(ReserveInventoryCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            EnsureSkuId(command.SkuId);

            var item = await _inventoryItemRepository.GetBySkuIdAsync(command.SkuId, cancellationToken)
                ?? throw new KeyNotFoundException($"SKU '{command.SkuId}' 的库存总账不存在，无法锁定。");

            var reservation = item.Reserve(command.ExternalOrderNo, command.Quantity);

            await _inventoryItemRepository.SaveChangesAsync(cancellationToken);

            return reservation.ToResult();
        }

        /// <summary>
        /// 释放库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <exception cref="KeyNotFoundException">当预留 ID 对应的库存总账不存在时抛出。</exception>
        public async Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            var item = await _inventoryItemRepository.GetByReservationIdAsync(reservationId, cancellationToken)
                ?? throw new KeyNotFoundException($"未找到预留 ID '{reservationId}' 对应的库存总账。");

            item.ReleaseReservation(reservationId);

            await _inventoryItemRepository.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 扣减库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <exception cref="KeyNotFoundException">当预留 ID 对应的库存总账不存在时抛出。</exception>
        public async Task DeductReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            var item = await _inventoryItemRepository.GetByReservationIdAsync(reservationId, cancellationToken)
                ?? throw new KeyNotFoundException($"未找到预留 ID '{reservationId}' 对应的库存总账。");

            item.DeductReservation(reservationId);

            await _inventoryItemRepository.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 校验 SKU 标识不能为空。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <exception cref="ArgumentException">当 SKU 标识为空或纯空白时抛出。</exception>
        private static void EnsureSkuId(string? skuId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));
            }
        }
    }
}
