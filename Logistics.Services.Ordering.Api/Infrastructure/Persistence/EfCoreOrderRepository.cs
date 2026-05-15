using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    public class EfCoreOrderRepository : IOrderRepository
    {
        private readonly OrderingDbContext _dbContext;

        public EfCoreOrderRepository(OrderingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
        }

        public async Task<IReadOnlyCollection<Order>> SearchAsync(OrderQuery orderQuery)
        {
            ArgumentNullException.ThrowIfNull(orderQuery);

            var query = _dbContext.Orders
                .AsNoTracking()
                .AsQueryable();

            if (orderQuery.Status.HasValue)
            {
                query = query.Where(order => order.Status == orderQuery.Status.Value);
            }

            if (orderQuery.From.HasValue)
            {
                query = query.Where(order => order.CreatedAt >= orderQuery.From.Value);
            }

            if (orderQuery.To.HasValue)
            {
                query = query.Where(order => order.CreatedAt <= orderQuery.To.Value);
            }

            if (!string.IsNullOrWhiteSpace(orderQuery.ExternalOrderNo))
            {
                var externalOrderNo = orderQuery.ExternalOrderNo.Trim();

                query = query.Where(order => order.ExternalOrderNo == externalOrderNo);
            }

            return await query
                .Include(order => order.Lines)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Orders
                .Include(order => order.Lines)
                .Include(order => order.TimelineItems)
                .FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
