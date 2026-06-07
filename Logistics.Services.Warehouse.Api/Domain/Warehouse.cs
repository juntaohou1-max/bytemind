namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 仓库聚合根，管理系统中的一个物理仓库及其内部区域划分。
    /// </summary>
    public class Warehouse
    {
        private readonly List<Zone> _zones = [];

        /// <summary>
        /// 仓库在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 仓库编码，用于业务侧快速识别，例如 "WH-SH-001"。
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        /// <summary>
        /// 仓库名称，例如 "上海一号仓"。
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// 仓库物理地址。
        /// </summary>
        public string Address { get; private set; } = string.Empty;

        /// <summary>
        /// 仓库当前状态，控制是否允许业务操作。
        /// </summary>
        public WarehouseStatus Status { get; private set; }

        /// <summary>
        /// 租户标识，用于多租户场景的数据隔离。
        /// </summary>
        public string TenantId { get; private set; } = string.Empty;

        /// <summary>
        /// 仓库记录创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// 仓库记录最后一次更新时间。
        /// </summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// 当前仓库包含的区域集合。
        /// </summary>
        public IReadOnlyCollection<Zone> Zones => _zones.AsReadOnly();

        /// <summary>
        /// 创建一个仓库。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="code">仓库编码。</param>
        /// <param name="name">仓库名称。</param>
        /// <param name="address">仓库物理地址。</param>
        public Warehouse(
            string tenantId,
            string code,
            string name,
            string address)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                throw new ArgumentException("租户标识不能为空。", nameof(tenantId));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("仓库编码不能为空。", nameof(code));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("仓库名称不能为空。", nameof(name));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("仓库地址不能为空。", nameof(address));

            Id = Guid.NewGuid();
            TenantId = tenantId.Trim();
            Code = code.Trim();
            Name = name.Trim();
            Address = address.Trim();
            Status = WarehouseStatus.Active;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private Warehouse()
        {
        }

        /// <summary>
        /// 向当前仓库添加一个区域。
        /// </summary>
        /// <param name="zone">要添加的区域实体。</param>
        public void AddZone(Zone zone)
        {
            if (zone is null)
                throw new ArgumentNullException(nameof(zone));

            if (_zones.Any(z => z.Name.Equals(zone.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"仓库中已存在同名区域：{zone.Name}。");

            _zones.Add(zone);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 停用仓库，只有 <see cref="WarehouseStatus.Active"/> 状态的仓库可以停用。
        /// </summary>
        public void Deactivate()
        {
            if (Status != WarehouseStatus.Active)
                throw new InvalidOperationException(
                    $"只有正常运营中的仓库可以停用，当前状态：{Status}。");

            Status = WarehouseStatus.Inactive;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 重新启用仓库，只有 <see cref="WarehouseStatus.Inactive"/> 状态的仓库可以启用。
        /// </summary>
        public void Activate()
        {
            if (Status != WarehouseStatus.Inactive)
                throw new InvalidOperationException(
                    $"只有已停用的仓库可以重新启用，当前状态：{Status}。");

            Status = WarehouseStatus.Active;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
