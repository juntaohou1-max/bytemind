namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;

        public string ExternalOrderNo { get; set; } = string.Empty;

        public AddressResponse ReceiverAddress { get; set; } = new();

        public List<OrderLineResponse> Lines { get; set; } = new();

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
    }
}
