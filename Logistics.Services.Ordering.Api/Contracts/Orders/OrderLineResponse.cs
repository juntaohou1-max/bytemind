namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class OrderLineResponse
    {
        public string SkuId { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
