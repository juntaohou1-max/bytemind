using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Application.IntegrationEvents
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken);
    }
}
