using Domain = Logistics.Services.Warehouse.Api.Domain;

namespace Logistics.Services.Warehouse.Api.Repositories
{
    /// <summary>
    /// 仓库仓储接口，用于封装 Warehouse 聚合根的持久化操作。
    /// </summary>
    public interface IWarehouseRepository
    {
        /// <summary>
        /// 新增仓库。
        /// </summary>
        /// <param name="warehouse">要保存的仓库聚合根。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        Task AddAsync(Domain.Warehouse warehouse, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据仓库 ID 查询仓库，同时加载区域集合及每个区域下的货位集合。
        /// </summary>
        /// <param name="id">仓库 ID。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>找到则返回仓库，找不到则返回 null。</returns>
        Task<Domain.Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 保存已跟踪仓库聚合的变更。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
