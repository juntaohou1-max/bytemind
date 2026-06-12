using Domain = Logistics.Services.Warehouse.Api.Domain;
using Logistics.Services.Warehouse.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Warehouse.Api.Infrastructure.Persistence
{
    /// <summary>
    /// 基于 EF Core 的仓库仓储实现。
    /// </summary>
    public class EfCoreWarehouseRepository : IWarehouseRepository
    {
        private readonly WarehouseDbContext _dbContext;

        /// <summary>
        /// 创建 EF Core 仓库仓储。
        /// </summary>
        /// <param name="dbContext">Warehouse 数据库上下文。</param>
        public EfCoreWarehouseRepository(WarehouseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 新增仓库。
        /// </summary>
        /// <param name="warehouse">要保存的仓库聚合根。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task AddAsync(Domain.Warehouse warehouse, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(warehouse);

            await _dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
        }

        /// <summary>
        /// 根据仓库 ID 查询仓库，同时加载区域集合及每个区域下的货位集合。
        /// </summary>
        /// <param name="id">仓库 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回仓库，找不到则返回 null。</returns>
        public Task<Domain.Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Warehouses
                .Include(w => w.Zones)
                .ThenInclude(z => z.Bins)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        /// <summary>
        /// 保存已跟踪仓库聚合的变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
