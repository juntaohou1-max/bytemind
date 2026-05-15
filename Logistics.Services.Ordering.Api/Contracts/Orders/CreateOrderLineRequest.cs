namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class CreateOrderLineRequest
    {
        /// <summary>
        /// 要发货的 SKU 标识。
        /// </summary>
        public string? SkuId { get; set; }

        /// <summary>
        /// 要发货的数量。
        /// </summary>
        public int Quantity { get; set; }
    }

}
