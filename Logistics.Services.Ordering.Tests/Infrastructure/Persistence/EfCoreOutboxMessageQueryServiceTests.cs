using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Tests.Infrastructure.Persistence
{
    public class EfCoreOutboxMessageQueryServiceTests
    {
        [Fact]
        public async Task GetAll_ShouldFilterByStatusAndReturnPagedResponse()
        {
            await using var dbContext = CreateDbContext();
            var pendingMessage = CreateMessage("OrderCreatedIntegrationEvent", DateTimeOffset.UtcNow.AddMinutes(-2));
            var failedMessage = CreateMessage("OrderCancelledIntegrationEvent", DateTimeOffset.UtcNow.AddMinutes(-1));
            failedMessage.MarkFailed();
            await dbContext.OutboxMessages.AddRangeAsync(pendingMessage, failedMessage);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var response = await service.GetAllAsync(
                "Failed",
                pageNumber: 1,
                pageSize: 20,
                sort: "occurredAtDesc");

            var item = Assert.Single(response.Items);
            Assert.Equal(failedMessage.Id, item.Id);
            Assert.Equal("Failed", item.Status);
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(20, response.PageSize);
            Assert.Equal(1, response.TotalCount);
        }

        [Fact]
        public async Task GetAll_ShouldSortAndPageByOccurredAt()
        {
            await using var dbContext = CreateDbContext();
            await dbContext.OutboxMessages.AddRangeAsync(
                CreateMessage("EventA", DateTimeOffset.Parse("2026-05-16T01:00:00Z")),
                CreateMessage("EventB", DateTimeOffset.Parse("2026-05-16T02:00:00Z")),
                CreateMessage("EventC", DateTimeOffset.Parse("2026-05-16T03:00:00Z")));
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var response = await service.GetAllAsync(
                status: null,
                pageNumber: 2,
                pageSize: 1,
                sort: "occurredAtAsc");

            var item = Assert.Single(response.Items);
            Assert.Equal("EventB", item.EventType);
            Assert.Equal(2, response.PageNumber);
            Assert.Equal(1, response.PageSize);
            Assert.Equal(3, response.TotalCount);
            Assert.Equal(3, response.TotalPages);
        }

        [Fact]
        public async Task GetAll_ShouldThrowException_WhenStatusIsInvalid()
        {
            await using var dbContext = CreateDbContext();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetAllAsync(
                    "Unknown",
                    pageNumber: 1,
                    pageSize: 20,
                    sort: "occurredAtDesc"));

            Assert.Equal("status", exception.ParamName);
        }

        [Fact]
        public async Task GetAll_ShouldThrowException_WhenPageSizeIsTooLarge()
        {
            await using var dbContext = CreateDbContext();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetAllAsync(
                    status: null,
                    pageNumber: 1,
                    pageSize: 101,
                    sort: "occurredAtDesc"));

            Assert.Equal("pageSize", exception.ParamName);
        }

        [Fact]
        public async Task GetById_ShouldReturnDetailWithPayload_WhenMessageExists()
        {
            await using var dbContext = CreateDbContext();
            var message = CreateMessage("OrderCreatedIntegrationEvent", DateTimeOffset.UtcNow);
            await dbContext.OutboxMessages.AddAsync(message);
            await dbContext.SaveChangesAsync();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var response = await service.GetByIdAsync(message.Id);

            Assert.NotNull(response);
            Assert.Equal(message.Id, response.Id);
            Assert.Equal(message.EventType, response.EventType);
            Assert.Equal(message.Payload, response.Payload);
            Assert.Equal("Pending", response.Status);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenMessageDoesNotExist()
        {
            await using var dbContext = CreateDbContext();
            var service = new EfCoreOutboxMessageQueryService(dbContext);

            var response = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(response);
        }

        private static OrderingDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new OrderingDbContext(options);
        }

        private static OutboxMessage CreateMessage(string eventType, DateTimeOffset occurredAt)
        {
            return new OutboxMessage(
                eventType,
                "{\"id\":\"test\"}",
                occurredAt);
        }
    }
}
