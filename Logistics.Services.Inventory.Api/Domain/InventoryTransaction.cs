namespace Logistics.Services.Inventory.Api.Domain
{
    /// <summary>
    /// 库存流水，用于记录一次库存相关动作。
    /// </summary>
    public class InventoryTransaction
    {
        /// <summary>
        /// 库存流水在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 发生库存变化的 SKU 标识。
        /// </summary>
        public string SkuId { get; private set; } = string.Empty;

        /// <summary>
        /// 库存流水类型。
        /// </summary>
        public InventoryTransactionType Type { get; private set; }

        /// <summary>
        /// 本次库存动作记录的数量变化。
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// 关联的库存预留标识；库存调整等动作可以为空。
        /// </summary>
        public Guid? ReservationId { get; private set; }

        /// <summary>
        /// 外部参考号，例如盘点单号、入库单号或外部订单号。
        /// </summary>
        public string? ReferenceNo { get; private set; }

        /// <summary>
        /// 库存流水创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// 创建一条库存流水。
        /// </summary>
        /// <param name="skuId">发生库存变化的 SKU 标识。</param>
        /// <param name="type">库存流水类型。</param>
        /// <param name="quantity">本次库存动作记录的数量变化。</param>
        /// <param name="reservationId">关联的库存预留标识。</param>
        /// <param name="referenceNo">外部参考号。</param>
        public InventoryTransaction(
            string skuId,
            InventoryTransactionType type,
            int quantity,
            Guid? reservationId = null,
            string? referenceNo = null)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));
            }

            Id = Guid.NewGuid();
            SkuId = skuId.Trim();
            Type = type;
            Quantity = quantity;
            ReservationId = reservationId;
            ReferenceNo = referenceNo?.Trim();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private InventoryTransaction()
        {
        }
    }
}
