using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Tests.Infrastructure.Persistence
{
    public class EfCoreOutboxMessageOperationServiceTests
    {
        [Fact]
        public async Task Retry_ShouldMoveFailedMessageToPending()
        {
            await using var dbContext = CreateDbContext();
            var message = CreateMessage(DateTimeOffset.UtcNow);
            message.MarkFailed();
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            var response = await service.RetryAsync(message.Id);

            Assert.NotNull(response);
            Assert.Equal(message.Id, response.Id);
            Assert.Equal("Pending", response.Status);
            Assert.Equal(1, response.RetryCount);
            Assert.Equal(OutboxStatus.Pending, message.Status);
        }

        [Fact]
        public async Task Retry_ShouldReturnNull_WhenMessageDoesNotExist()
        {
            await using var dbContext = CreateDbContext();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            var response = await service.RetryAsync(Guid.NewGuid());

            Assert.Null(response);
        }

        [Fact]
        public async Task Retry_ShouldThrowException_WhenMessageIsNotFailed()
        {
            await using var dbContext = CreateDbContext();
            var message = CreateMessage(DateTimeOffset.UtcNow);
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RetryAsync(message.Id));
        }

        [Fact]
        public async Task RetryFailed_ShouldMoveFailedMessagesToPending()
        {
            await using var dbContext = CreateDbContext();
            var firstFailedMessage = CreateMessage(DateTimeOffset.Parse("2026-05-16T01:00:00Z"));
            var secondFailedMessage = CreateMessage(DateTimeOffset.Parse("2026-05-16T02:00:00Z"));
            var pendingMessage = CreateMessage(DateTimeOffset.Parse("2026-05-16T03:00:00Z"));
            firstFailedMessage.MarkFailed();
            secondFailedMessage.MarkFailed();
            await dbContext.OutboxMessages.AddRangeAsync(
                firstFailedMessage,
                secondFailedMessage,
                pendingMessage);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            var response = await service.RetryFailedAsync(take: 20);

            Assert.Equal(2, response.RetriedCount);
            Assert.Equal(2, response.Items.Count);
            Assert.Equal(OutboxStatus.Pending, firstFailedMessage.Status);
            Assert.Equal(OutboxStatus.Pending, secondFailedMessage.Status);
            Assert.Equal(OutboxStatus.Pending, pendingMessage.Status);
        }

        [Fact]
        public async Task RetryFailed_ShouldRespectTake()
        {
            await using var dbContext = CreateDbContext();
            var firstFailedMessage = CreateMessage(DateTimeOffset.Parse("2026-05-16T01:00:00Z"));
            var secondFailedMessage = CreateMessage(DateTimeOffset.Parse("2026-05-16T02:00:00Z"));
            firstFailedMessage.MarkFailed();
            secondFailedMessage.MarkFailed();
            await dbContext.OutboxMessages.AddRangeAsync(firstFailedMessage, secondFailedMessage);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            var response = await service.RetryFailedAsync(take: 1);

            Assert.Equal(1, response.RetriedCount);
            Assert.Equal(OutboxStatus.Pending, firstFailedMessage.Status);
            Assert.Equal(OutboxStatus.Failed, secondFailedMessage.Status);
        }

        [Fact]
        public async Task RetryFailed_ShouldThrowException_WhenTakeIsInvalid()
        {
            await using var dbContext = CreateDbContext();
            var service = new EfCoreOutboxMessageOperationService(dbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.RetryFailedAsync(take: 0));

            Assert.Equal("take", exception.ParamName);
        }

        private static OrderingDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OrderingDbContext(options);
        }

        private static OutboxMessage CreateMessage(DateTimeOffset occurredAt)
        {
            return new OutboxMessage(
                "OrderCreatedIntegrationEvent",
                "{\"id\":\"test\"}",
                occurredAt);
        }
    }
}
