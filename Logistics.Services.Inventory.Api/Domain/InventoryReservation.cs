namespace Logistics.Services.Inventory.Api.Domain
{
    /// <summary>
    /// 库存预留，表示某个外部订单对某个 SKU 的库存占用。
    /// </summary>
    public class InventoryReservation
    {
        /// <summary>
        /// 库存预留在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 外部系统传入的订单号。
        /// </summary>
        public string ExternalOrderNo { get; private set; } = string.Empty;

        /// <summary>
        /// 被锁定库存的 SKU 标识。
        /// </summary>
        public string SkuId { get; private set; } = string.Empty;

        /// <summary>
        /// 本次锁定的库存数量。
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// 库存预留当前状态。
        /// </summary>
        public InventoryReservationStatus Status { get; private set; }

        /// <summary>
        /// 库存预留创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// 创建一条库存预留。
        /// </summary>
        /// <param name="externalOrderNo">外部系统传入的订单号。</param>
        /// <param name="skuId">被锁定库存的 SKU 标识。</param>
        /// <param name="quantity">本次锁定的库存数量。</param>
        public InventoryReservation(
            string externalOrderNo,
            string skuId,
            int quantity)
        {

            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));

            if (string.IsNullOrWhiteSpace(externalOrderNo))
                throw new ArgumentException("外部系统订单号 不能为空。", nameof(externalOrderNo));

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "锁定数量必须大于 0。");

            SkuId = skuId.Trim();
            Id = Guid.NewGuid();
            ExternalOrderNo = externalOrderNo.Trim();
            Quantity = quantity;
            Status = InventoryReservationStatus.Active;
            CreatedAt = DateTimeOffset.Now;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private InventoryReservation()
        {
        }

        /// <summary>
        /// 将库存预留标记为已释放。
        /// </summary>
        public void Release()
        {
            if (Status != InventoryReservationStatus.Active)
            {
                throw new InvalidOperationException("只有Active订单才能标记为已释放。");
            }

            Status = InventoryReservationStatus.Released;
        }

        /// <summary>
        /// 将库存预留标记为已扣减。
        /// </summary>
        public void Deduct()
        {
            if (Status != InventoryReservationStatus.Active)
            {
                throw new InvalidOperationException("只有Active订单才能标记为已扣减。");
            }

            Status = InventoryReservationStatus.Deducted;
        }
    }
}
