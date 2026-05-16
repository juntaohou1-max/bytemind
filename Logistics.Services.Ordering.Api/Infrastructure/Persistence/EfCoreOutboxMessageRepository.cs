using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    public class EfCoreOutboxMessageRepository : IOutboxMessageRepository
    {

        private readonly OrderingDbContext _dbContext;

        public EfCoreOutboxMessageRepository(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(OutboxMessage outboxMessage)
        {
            ArgumentNullException.ThrowIfNull(outboxMessage);

            await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        }
    }
}
