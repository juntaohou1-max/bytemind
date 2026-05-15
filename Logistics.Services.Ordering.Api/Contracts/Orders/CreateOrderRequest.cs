namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class CreateOrderRequest
    {
        public string? TenantId { get; set; }

        public string? CustomerId { get; set; }

        public string? ExternalOrderNo { get; set; }

        public AddressRequest? ReceiverAddress { get; set; }

        public List<CreateOrderLineRequest>? Lines { get; set; }
    }
}
