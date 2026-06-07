namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 区域实体，表示仓库内部的一个功能分区，例如拣货区、存储区等。
    /// </summary>
    public class Zone
    {
        private readonly List<BinLocation> _bins = [];

        /// <summary>
        /// 区域在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 当前区域所属的仓库标识。
        /// </summary>
        public Guid WarehouseId { get; private set; }

        /// <summary>
        /// 区域名称，例如 "A区-拣货区"。
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// 区域类型，定义当前区域的用途分类。
        /// </summary>
        public ZoneType ZoneType { get; private set; }

        /// <summary>
        /// 当前区域包含的货位集合。
        /// </summary>
        public IReadOnlyCollection<BinLocation> Bins => _bins.AsReadOnly();

        /// <summary>
        /// 创建一个区域。
        /// </summary>
        /// <param name="warehouseId">所属仓库标识。</param>
        /// <param name="name">区域名称。</param>
        /// <param name="zoneType">区域类型。</param>
        public Zone(Guid warehouseId, string name, ZoneType zoneType)
        {
            if (warehouseId == Guid.Empty)
                throw new ArgumentException("仓库标识不能为空。", nameof(warehouseId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("区域名称不能为空。", nameof(name));

            if (!Enum.IsDefined(zoneType))
                throw new ArgumentOutOfRangeException(nameof(zoneType), "区域类型值无效。");

            Id = Guid.NewGuid();
            WarehouseId = warehouseId;
            Name = name.Trim();
            ZoneType = zoneType;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private Zone()
        {
        }

        /// <summary>
        /// 向当前区域添加一个货位。
        /// </summary>
        /// <param name="bin">要添加的货位实体。</param>
        public void AddBin(BinLocation bin)
        {
            if (bin is null)
                throw new ArgumentNullException(nameof(bin));

            if (_bins.Any(b => b.Code.Equals(bin.Code, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"区域中已存在相同编码的货位：{bin.Code}。");

            _bins.Add(bin);
        }
    }
}
