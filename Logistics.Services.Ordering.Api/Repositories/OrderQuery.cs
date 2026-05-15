using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    public class OrderQuery
    {
        public OrderStatus? Status { get; init; }

        public DateTimeOffset? From { get; init; }

        public DateTimeOffset? To { get; init; }

        public string? ExternalOrderNo { get; init; }
    }
}
