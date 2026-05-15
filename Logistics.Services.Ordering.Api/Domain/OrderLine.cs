namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// 订单明细。
    /// </summary>
    /// <remarks>
    /// 一条明细表示本次发货指令中某个 SKU 需要发出的数量。
    /// 它属于 <see cref="Order"/>，第一阶段不作为独立聚合根处理。
    /// </remarks>
    public class OrderLine
    {
        /// <summary>
        /// 要发货的 SKU 标识。
        /// </summary>
        public string SkuId { get; private set; } = string.Empty;

        /// <summary>
        /// 要发货的数量。
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// 创建订单明细。
        /// </summary>
        /// <param name="skuId">要发货的 SKU 标识。</param>
        /// <param name="quantity">要发货的数量，必须大于 0。</param>
        /// <exception cref="ArgumentException">SKU 标识为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">数量小于等于 0 时抛出。</exception>
        public OrderLine(string skuId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(skuId))
            {
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "订单明细数量必须大于 0。");
            }

            SkuId = skuId.Trim();
            Quantity = quantity;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private OrderLine()
        {
        }
    }
}
