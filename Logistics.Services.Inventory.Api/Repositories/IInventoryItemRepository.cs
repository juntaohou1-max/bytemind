using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Api.Repositories
{
    /// <summary>
    /// 库存总账仓储接口，用于封装 InventoryItem 聚合的持久化操作。
    /// </summary>
    public interface IInventoryItemRepository
    {
        /// <summary>
        /// 新增库存总账。
        /// </summary>
        /// <param name="item">要保存的库存总账。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据库存总账 ID 查询库存总账。
        /// </summary>
        /// <param name="id">库存总账 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据 SKU 查询库存总账。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        Task<InventoryItem?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据库存预留 ID 查询所属库存总账，并加载预留集合。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        Task<InventoryItem?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存已跟踪库存聚合的变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
