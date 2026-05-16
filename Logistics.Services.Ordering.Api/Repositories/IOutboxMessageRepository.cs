using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Repositories
{
    public interface IOutboxMessageRepository
    {
        Task AddAsync(OutboxMessage outboxMessage);
    }
}
