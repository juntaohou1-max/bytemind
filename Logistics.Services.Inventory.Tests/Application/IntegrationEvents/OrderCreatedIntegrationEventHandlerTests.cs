using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Application.IntegrationEvents;
using Logistics.Services.Inventory.Api.Infrastructure.Inbox;
using Logistics.Services.Inventory.Api.Infrastructure.Outbox;
using Logistics.Services.Inventory.Api.Infrastructure.Persistence;
using Logistics.Services.Inventory.Api.IntegrationEvents;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Inventory.Tests.Application.IntegrationEvents
{
    /// <summary>
    /// 订单已创建事件处理器测试，验证 Inbox 幂等、库存锁定编排和 Outbox 写入逻辑。
    /// </summary>
    public class OrderCreatedIntegrationEventHandlerTests
    {
        /// <summary>
        /// 正常处理事件时，应该为每个订单明细行调用一次库存锁定。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldReserveInventoryForEachLine_WhenEventIsNew()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var @event = CreateEvent(
                eventId: Guid.NewGuid(),
                externalOrderNo: "ERP-001",
                lines:
                [
                    new OrderCreatedIntegrationEventLine("SKU-001", 10),
                    new OrderCreatedIntegrationEventLine("SKU-002", 5)
                ]);

            // Act
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
            }

            // Assert
            Assert.Equal(2, fakeService.ReserveCommands.Count);
            Assert.Equal("SKU-001", fakeService.ReserveCommands[0].SkuId);
            Assert.Equal("ERP-001", fakeService.ReserveCommands[0].ExternalOrderNo);
            Assert.Equal(10, fakeService.ReserveCommands[0].Quantity);
            Assert.Equal("SKU-002", fakeService.ReserveCommands[1].SkuId);
            Assert.Equal("ERP-001", fakeService.ReserveCommands[1].ExternalOrderNo);
            Assert.Equal(5, fakeService.ReserveCommands[1].Quantity);
        }

        /// <summary>
        /// 正常处理事件后，Inbox 中应该有一条对应的处理记录。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldWriteInboxMessage_WhenEventIsProcessed()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var eventId = Guid.NewGuid();
            var @event = CreateEvent(eventId, "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);

            // Act
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
            }

            // Assert
            await using (var dbContext = new InventoryDbContext(options))
            {
                var existsInInbox = await dbContext.InboxMessages
                    .AnyAsync(m => m.EventId == eventId);

                Assert.True(existsInInbox);
            }
        }

        /// <summary>
        /// 正常处理事件后，Outbox 中应该有一条 InventoryReserved 待发布消息。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldWriteOutboxMessage_WhenEventIsProcessed()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var @event = CreateEvent(Guid.NewGuid(), "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);

            // Act
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
            }

            // Assert：Outbox 中有一条 InventoryReserved 事件
            await using (var dbContext = new InventoryDbContext(options))
            {
                var outboxMessage = await dbContext.OutboxMessages.SingleOrDefaultAsync();

                Assert.NotNull(outboxMessage);
                Assert.Equal(InventoryReservedIntegrationEvent.TypeName, outboxMessage.EventType);
                Assert.Contains("ERP-001", outboxMessage.Payload);
            }
        }

        /// <summary>
        /// 重复处理同一个事件时，第二次应该幂等跳过，不再调用库存锁定。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldSkipDuplicateEvent_WhenEventAlreadyProcessed()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var eventId = Guid.NewGuid();
            var @event = CreateEvent(eventId, "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);

            // Act — 第一次
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
            }

            var reserveCountAfterFirst = fakeService.ReserveCommands.Count;

            // Act — 第二次（同样 EventId）
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
            }

            // Assert：第二次处理没有新增锁定调用
            Assert.Equal(1, reserveCountAfterFirst);
            Assert.Equal(reserveCountAfterFirst, fakeService.ReserveCommands.Count);
        }

        /// <summary>
        /// 重复处理同一个事件时，Inbox 中应该只有一条记录。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldNotWriteDuplicateInboxMessage_WhenEventAlreadyProcessed()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var eventId = Guid.NewGuid();
            var @event = CreateEvent(eventId, "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);

            // Act — 处理两次
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
                await handler.HandleAsync(@event);
            }

            // Assert：Inbox 中只有一条记录
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxCount = await dbContext.InboxMessages
                    .CountAsync(m => m.EventId == eventId);

                Assert.Equal(1, inboxCount);
            }
        }

        /// <summary>
        /// 重复处理同一个事件时，Outbox 中也应该只有一条记录。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldNotWriteDuplicateOutboxMessage_WhenEventAlreadyProcessed()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var @event = CreateEvent(Guid.NewGuid(), "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);

            // Act — 处理两次
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(@event);
                await handler.HandleAsync(@event);
            }

            // Assert：Outbox 中只有一条记录
            await using (var dbContext = new InventoryDbContext(options))
            {
                var outboxCount = await dbContext.OutboxMessages.CountAsync();
                Assert.Equal(1, outboxCount);
            }
        }

        /// <summary>
        /// 不同事件应该各自独立处理，互不影响。
        /// </summary>
        [Fact]
        public async Task HandleAsync_ShouldProcessDifferentEventsIndependently()
        {
            // Arrange
            var options = CreateOptions();
            var fakeService = new FakeInventoryApplicationService();
            var event1 = CreateEvent(Guid.NewGuid(), "ERP-001", [new OrderCreatedIntegrationEventLine("SKU-001", 10)]);
            var event2 = CreateEvent(Guid.NewGuid(), "ERP-002", [new OrderCreatedIntegrationEventLine("SKU-002", 20)]);

            // Act
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxRepo = new EfCoreInboxMessageRepository(dbContext);
                var outboxRepo = new EfCoreOutboxMessageRepository(dbContext);
                var handler = new OrderCreatedIntegrationEventHandler(fakeService, inboxRepo, outboxRepo, dbContext);

                await handler.HandleAsync(event1);
                await handler.HandleAsync(event2);
            }

            // Assert：两次事件都处理了
            Assert.Equal(2, fakeService.ReserveCommands.Count);
            Assert.Contains(fakeService.ReserveCommands, c => c.SkuId == "SKU-001" && c.Quantity == 10);
            Assert.Contains(fakeService.ReserveCommands, c => c.SkuId == "SKU-002" && c.Quantity == 20);

            // Assert：Inbox 中有两条记录
            await using (var dbContext = new InventoryDbContext(options))
            {
                var inboxCount = await dbContext.InboxMessages.CountAsync();
                Assert.Equal(2, inboxCount);

                var outboxCount = await dbContext.OutboxMessages.CountAsync();
                Assert.Equal(2, outboxCount);
            }
        }

        /// <summary>
        /// 创建订单已创建集成事件测试数据。
        /// </summary>
        /// <param name="eventId">事件唯一标识。</param>
        /// <param name="externalOrderNo">外部订单号。</param>
        /// <param name="lines">订单明细行。</param>
        private static OrderCreatedIntegrationEvent CreateEvent(
            Guid eventId,
            string externalOrderNo,
            IReadOnlyCollection<OrderCreatedIntegrationEventLine> lines)
        {
            return new OrderCreatedIntegrationEvent(
                eventId,
                "tenant-1",
                Guid.NewGuid().ToString(),
                DateTimeOffset.UtcNow,
                null,
                1,
                externalOrderNo,
                "cust-001",
                lines);
        }

        /// <summary>
        /// 创建测试用 Inventory 数据库上下文配置。
        /// </summary>
        private static DbContextOptions<InventoryDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// 用于事件处理器测试的库存应用服务假实现。
        /// </summary>
        private sealed class FakeInventoryApplicationService : IInventoryApplicationService
        {
            /// <summary>
            /// 记录每次 ReserveInventoryAsync 收到的命令。
            /// </summary>
            public List<ReserveInventoryCommand> ReserveCommands { get; } = [];

            /// <summary>
            /// 根据 SKU 查询库存总账。
            /// </summary>
            /// <param name="skuId">SKU 标识。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItemResult?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<InventoryItemResult?>(null);
            }

            /// <summary>
            /// 调整库存。
            /// </summary>
            /// <param name="command">调整库存命令。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItemResult> AdjustInventoryAsync(AdjustInventoryCommand command, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new InventoryItemResult(Guid.NewGuid(), command.SkuId, 0, 0, 0, 0));
            }

            /// <summary>
            /// 锁定库存，记录命令并返回假结果。
            /// </summary>
            /// <param name="command">锁定库存命令。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryReservationResult> ReserveInventoryAsync(ReserveInventoryCommand command, CancellationToken cancellationToken = default)
            {
                ReserveCommands.Add(command);

                return Task.FromResult(new InventoryReservationResult(
                    Guid.NewGuid(),
                    command.ExternalOrderNo,
                    command.SkuId,
                    command.Quantity,
                    "Active",
                    DateTimeOffset.UtcNow));
            }

            /// <summary>
            /// 释放库存预留。
            /// </summary>
            /// <param name="reservationId">库存预留 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            /// <summary>
            /// 扣减库存预留。
            /// </summary>
            /// <param name="reservationId">库存预留 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task DeductReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }
    }
}
