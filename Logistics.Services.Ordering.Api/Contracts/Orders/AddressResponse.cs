namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class AddressResponse
    {
        public string ReceiverName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Province { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;
    }
}
