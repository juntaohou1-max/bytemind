using Logistics.Services.Inventory.Api.Domain;
using Logistics.Services.Inventory.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Logistics.Services.Inventory.Api.Infrastructure.Persistence
{
    /// <summary>
    /// 基于 EF Core 的库存总账仓储实现。
    /// </summary>
    public class EfCoreInventoryItemRepository : IInventoryItemRepository
    {
        private readonly InventoryDbContext _dbContext;

        /// <summary>
        /// 创建 EF Core 库存总账仓储。
        /// </summary>
        /// <param name="dbContext">Inventory 数据库上下文。</param>
        public EfCoreInventoryItemRepository(InventoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 新增库存总账。
        /// </summary>
        /// <param name="item">要保存的库存总账。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(item);

            await _dbContext.InventoryItems.AddAsync(item, cancellationToken);
        }

        /// <summary>
        /// 根据库存总账 ID 查询库存总账。
        /// </summary>
        /// <param name="id">库存总账 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
           return  _dbContext.InventoryItems
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        }

        /// <summary>
        /// 根据 SKU 查询库存总账。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        public Task<InventoryItem?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));

            return _dbContext.InventoryItems
                .FirstOrDefaultAsync(item => item.SkuId == skuId.Trim(), cancellationToken);
        }

        /// <summary>
        /// 根据库存预留 ID 查询所属库存总账，并加载预留集合。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回库存总账，找不到则返回 null。</returns>
        public Task<InventoryItem?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            return _dbContext.InventoryItems
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(item =>
                item.Reservations.Any(reservation => reservation.Id == reservationId),
                cancellationToken);
        }

        /// <summary>
        /// 保存已跟踪库存聚合的变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
           await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
