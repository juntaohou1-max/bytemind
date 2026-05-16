using Logistics.Services.Ordering.Api.Contracts;
using Logistics.Services.Ordering.Api.Contracts.OutboxMessages;

namespace Logistics.Services.Ordering.Api.Application.OutboxMessages
{
    public interface IOutboxMessageQueryService
    {
        Task<PagedResponse<OutboxMessageListItemResponse>> GetAllAsync(
            string? status,
            int pageNumber,
            int pageSize,
            string sort,
            CancellationToken cancellationToken = default);

        Task<OutboxMessageDetailResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
