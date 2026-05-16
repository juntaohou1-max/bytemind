using Logistics.Services.Ordering.Api.Application.OutboxMessages;
using Logistics.Services.Ordering.Api.Contracts;
using Logistics.Services.Ordering.Api.Contracts.OutboxMessages;
using Logistics.Services.Ordering.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    public class EfCoreOutboxMessageQueryService : IOutboxMessageQueryService
    {
        private readonly OrderingDbContext _dbContext;

        public EfCoreOutboxMessageQueryService(OrderingDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<PagedResponse<OutboxMessageListItemResponse>> GetAllAsync(
            string? status,
            int pageNumber,
            int pageSize,
            string sort,
            CancellationToken cancellationToken = default)
        {
            var messageStatus = ParseStatus(status);
            ValidatePaging(pageNumber, pageSize);
            ValidateSort(sort);

            var query = _dbContext.OutboxMessages
                .AsNoTracking()
                .AsQueryable();

            if (messageStatus.HasValue)
            {
                query = query.Where(message => message.Status == messageStatus.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = string.Equals(sort, "occurredAtAsc", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(message => message.OccurredAt).ThenBy(message => message.Id)
                : query.OrderByDescending(message => message.OccurredAt).ThenBy(message => message.Id);

            var messages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(message => new
                {
                    message.Id,
                    message.EventType,
                    message.Status,
                    message.OccurredAt,
                    message.ProcessedAt,
                    message.RetryCount
                })
                .ToListAsync(cancellationToken);

            var items = messages
                .Select(message => new OutboxMessageListItemResponse
                {
                    Id = message.Id,
                    EventType = message.EventType,
                    Status = message.Status.ToString(),
                    OccurredAt = message.OccurredAt,
                    ProcessedAt = message.ProcessedAt,
                    RetryCount = message.RetryCount
                })
                .ToList();

            return new PagedResponse<OutboxMessageListItemResponse>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<OutboxMessageDetailResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var message = await _dbContext.OutboxMessages
                .AsNoTracking()
                .Where(message => message.Id == id)
                .Select(message => new
                {
                    message.Id,
                    message.EventType,
                    message.Payload,
                    message.Status,
                    message.OccurredAt,
                    message.ProcessedAt,
                    message.RetryCount
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (message is null)
            {
                return null;
            }

            return new OutboxMessageDetailResponse
            {
                Id = message.Id,
                EventType = message.EventType,
                Payload = message.Payload,
                Status = message.Status.ToString(),
                OccurredAt = message.OccurredAt,
                ProcessedAt = message.ProcessedAt,
                RetryCount = message.RetryCount
            };
        }

        private static OutboxStatus? ParseStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            if (!Enum.TryParse<OutboxStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                throw new ArgumentException("Outbox status is invalid.", nameof(status));
            }

            return parsedStatus;
        }

        private static void ValidatePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than or equal to 1.", nameof(pageNumber));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("Page size must be greater than or equal to 1.", nameof(pageSize));
            }

            if (pageSize > 100)
            {
                throw new ArgumentException("Page size cannot be greater than 100.", nameof(pageSize));
            }
        }

        private static void ValidateSort(string sort)
        {
            if (!string.Equals(sort, "occurredAtDesc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sort, "occurredAtAsc", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Sort is invalid.", nameof(sort));
            }
        }
    }
}
