namespace Logistics.Services.Warehouse.Api.Domain
{
    /// <summary>
    /// 货位实体，表示仓库区域内一个具体的存储位置，是仓库中最小的存储单元。
    /// </summary>
    public class BinLocation
    {
        /// <summary>
        /// 货位在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 当前货位所属的区域标识。
        /// </summary>
        public Guid ZoneId { get; private set; }

        /// <summary>
        /// 货位编码，采用"通道-货架-层"格式，例如 "A-01-03"。
        /// </summary>
        public string Code { get; private set; } = string.Empty;

        /// <summary>
        /// 当前货位的状态，控制是否可分配或拣货。
        /// </summary>
        public BinStatus Status { get; private set; }

        /// <summary>
        /// 当前货位中存放的 SKU 标识，为空表示该货位无商品。
        /// </summary>
        public string? SkuId { get; private set; }

        /// <summary>
        /// 创建一个货位。
        /// </summary>
        /// <param name="zoneId">所属区域标识。</param>
        /// <param name="code">货位编码，格式如 "A-01-03"。</param>
        public BinLocation(Guid zoneId, string code)
        {
            if (zoneId == Guid.Empty)
                throw new ArgumentException("区域标识不能为空。", nameof(zoneId));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("货位编码不能为空。", nameof(code));

            Id = Guid.NewGuid();
            ZoneId = zoneId;
            Code = code.Trim();
            Status = BinStatus.Available;
            SkuId = null;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private BinLocation()
        {
        }

        /// <summary>
        /// 将指定 SKU 分配到当前货位，货位状态变为 <see cref="BinStatus.Occupied"/>。
        /// </summary>
        /// <param name="skuId">要分配的 SKU 标识。</param>
        public void AssignSku(string skuId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));

            if (Status != BinStatus.Available)
                throw new InvalidOperationException(
                    $"只有可用状态的货位可以分配 SKU，当前状态：{Status}。");

            SkuId = skuId.Trim();
            Status = BinStatus.Occupied;
        }

        /// <summary>
        /// 清空当前货位中的 SKU，货位状态恢复为 <see cref="BinStatus.Available"/>。
        /// </summary>
        public void ClearSku()
        {
            if (Status != BinStatus.Occupied)
                throw new InvalidOperationException(
                    $"只有已占用状态的货位可以清空 SKU，当前状态：{Status}。");

            SkuId = null;
            Status = BinStatus.Available;
        }

        /// <summary>
        /// 将货位状态设置为 <see cref="BinStatus.Reserved"/>，用于出库任务预留。
        /// 只有已占用的货位可以被预留。
        /// </summary>
        public void Reserve()
        {
            if (Status != BinStatus.Occupied)
                throw new InvalidOperationException(
                    $"只有已占用状态的货位可以预留，当前状态：{Status}。");

            Status = BinStatus.Reserved;
        }

        /// <summary>
        /// 将货位状态设置为 <see cref="BinStatus.Blocked"/>，用于盘点、维修等场景。
        /// 任何非锁定状态的货位都可以被锁定。
        /// </summary>
        public void Block()
        {
            if (Status == BinStatus.Blocked)
                throw new InvalidOperationException("货位已经被锁定。");

            Status = BinStatus.Blocked;
        }

        /// <summary>
        /// 解除货位锁定，货位恢复为 <see cref="BinStatus.Available"/>。
        /// 只有已锁定状态的货位可以解锁。
        /// </summary>
        public void Unblock()
        {
            if (Status != BinStatus.Blocked)
                throw new InvalidOperationException(
                    $"只有已锁定状态的货位可以解锁，当前状态：{Status}。");

            SkuId = null;
            Status = BinStatus.Available;
        }
    }
}
