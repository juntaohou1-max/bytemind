using Logistics.Services.Ordering.Api.Application.IntegrationEvents;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Infrastructure.Outbox;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Logistics.Services.Ordering.Tests.Infrastructure.Outbox
{
    public class OutboxMessagePublisherTests
    {
        [Fact]
        public async Task ProcessOnce_ShouldMarkPendingMessageAsPublished_WhenPublishSucceeds()
        {
            var message = CreateMessage();
            var repository = new TestOutboxMessageRepository(message);
            var eventPublisher = new TestIntegrationEventPublisher();
            var publisher = CreatePublisher(repository, eventPublisher);

            await publisher.ProcessOnceAsync(CancellationToken.None);

            Assert.Equal(OutboxStatus.Published, message.Status);
            Assert.NotNull(message.ProcessedAt);
            Assert.Equal(0, message.RetryCount);
            Assert.Single(eventPublisher.PublishedMessages);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task ProcessOnce_ShouldMarkPendingMessageAsFailed_WhenPublishFails()
        {
            var message = CreateMessage();
            var repository = new TestOutboxMessageRepository(message);
            var eventPublisher = new TestIntegrationEventPublisher(shouldFail: true);
            var publisher = CreatePublisher(repository, eventPublisher);

            await publisher.ProcessOnceAsync(CancellationToken.None);

            Assert.Equal(OutboxStatus.Failed, message.Status);
            Assert.Null(message.ProcessedAt);
            Assert.Equal(1, message.RetryCount);
            Assert.Single(eventPublisher.PublishedMessages);
            Assert.Equal(1, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task ProcessOnce_ShouldRetryFailedMessage_WhenRetryLimitIsNotReached()
        {
            var message = CreateMessage();
            message.MarkFailed();
            var repository = new TestOutboxMessageRepository(message);
            var eventPublisher = new TestIntegrationEventPublisher();
            var publisher = CreatePublisher(repository, eventPublisher);

            await publisher.ProcessOnceAsync(CancellationToken.None);

            Assert.Equal(OutboxStatus.Published, message.Status);
            Assert.NotNull(message.ProcessedAt);
            Assert.Equal(1, message.RetryCount);
            Assert.Single(eventPublisher.PublishedMessages);
            Assert.Equal(2, repository.SaveChangesCallCount);
        }

        [Fact]
        public async Task ProcessOnce_ShouldNotRetryFailedMessage_WhenRetryLimitIsReached()
        {
            var message = CreateMessage();
            message.MarkFailed();
            message.MarkFailed();
            message.MarkFailed();
            var repository = new TestOutboxMessageRepository(message);
            var eventPublisher = new TestIntegrationEventPublisher();
            var publisher = CreatePublisher(repository, eventPublisher);

            await publisher.ProcessOnceAsync(CancellationToken.None);

            Assert.Equal(OutboxStatus.Failed, message.Status);
            Assert.Null(message.ProcessedAt);
            Assert.Equal(3, message.RetryCount);
            Assert.Empty(eventPublisher.PublishedMessages);
            Assert.Equal(0, repository.SaveChangesCallCount);
        }

        private static OutboxMessagePublisher CreatePublisher(
            TestOutboxMessageRepository repository,
            TestIntegrationEventPublisher eventPublisher)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOutboxMessageRepository>(repository);
            var serviceProvider = services.BuildServiceProvider();

            return new OutboxMessagePublisher(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                eventPublisher,
                NullLogger<OutboxMessagePublisher>.Instance);
        }

        private static OutboxMessage CreateMessage()
        {
            return new OutboxMessage(
                "OrderCreatedIntegrationEvent",
                "{\"orderId\":\"order-001\"}");
        }

        private sealed class TestIntegrationEventPublisher : IIntegrationEventPublisher
        {
            private readonly bool _shouldFail;

            public TestIntegrationEventPublisher(bool shouldFail = false)
            {
                _shouldFail = shouldFail;
            }

            public IReadOnlyCollection<OutboxMessage> PublishedMessages => _publishedMessages.AsReadOnly();

            private readonly List<OutboxMessage> _publishedMessages = [];

            public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _publishedMessages.Add(message);

                if (_shouldFail)
                {
                    throw new InvalidOperationException("Publish failed.");
                }

                return Task.CompletedTask;
            }
        }

        private sealed class TestOutboxMessageRepository : IOutboxMessageRepository
        {
            private readonly List<OutboxMessage> _messages;

            public TestOutboxMessageRepository(params OutboxMessage[] messages)
            {
                _messages = messages.ToList();
            }

            public int SaveChangesCallCount { get; private set; }

            public Task AddAsync(OutboxMessage outboxMessage)
            {
                _messages.Add(outboxMessage);

                return Task.CompletedTask;
            }

            public Task<IReadOnlyCollection<OutboxMessage>> GetMessagesByStatusAsync(
                OutboxStatus status,
                int take,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyCollection<OutboxMessage> messages = _messages
                    .Where(message => message.Status == status)
                    .OrderBy(message => message.OccurredAt)
                    .Take(take)
                    .ToList();

                return Task.FromResult(messages);
            }

            public Task<IReadOnlyCollection<OutboxMessage>> GetFailedMessagesForRetryAsync(
                int maxRetryCount,
                int take,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyCollection<OutboxMessage> messages = _messages
                    .Where(message =>
                        message.Status == OutboxStatus.Failed &&
                        message.RetryCount < maxRetryCount)
                    .OrderBy(message => message.OccurredAt)
                    .Take(take)
                    .ToList();

                return Task.FromResult(messages);
            }

            public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SaveChangesCallCount++;

                return Task.CompletedTask;
            }
        }
    }
}
