using System.Text.Json;
using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Domain.Inbox;
using Logistics.Services.Inventory.Api.Domain.Outbox;
using Logistics.Services.Inventory.Api.Infrastructure.Inbox;
using Logistics.Services.Inventory.Api.Infrastructure.Outbox;
using Logistics.Services.Inventory.Api.Infrastructure.Persistence;
using Logistics.Services.Inventory.Api.IntegrationEvents;

namespace Logistics.Services.Inventory.Api.Application.IntegrationEvents
{
    /// <summary>
    /// 订单已创建事件的处理器，负责在 Inventory 侧为订单明细锁定库存，并发出库存已锁定事件。
    /// </summary>
    /// <remarks>
    /// 收到 OrderCreatedIntegrationEvent 后：
    /// 1. 先查 Inbox 判断是否已处理（幂等）；
    /// 2. 遍历订单明细行，逐行锁定库存；
    /// 3. 全部锁定成功后写入 Inbox 记录（幂等凭证）；
    /// 4. 同时写入 Outbox 记录（InventoryReserved 事件，通知下游履约模块）。
    /// 如果任何一行锁定失败，Inbox 和 Outbox 都不会写入，下次重试时会重新处理。
    /// </remarks>
    public class OrderCreatedIntegrationEventHandler
    {
        private readonly IInventoryApplicationService _inventoryService;
        private readonly IInboxMessageRepository _inboxRepository;
        private readonly IOutboxMessageRepository _outboxRepository;
        private readonly InventoryDbContext _dbContext;

        /// <summary>
        /// 创建订单已创建事件处理器。
        /// </summary>
        /// <param name="inventoryService">库存应用服务，用于调用锁定库存用例。</param>
        /// <param name="inboxRepository">Inbox 仓储，用于幂等判断和写入处理记录。</param>
        /// <param name="outboxRepository">Outbox 仓储，用于写入待发布的集成事件。</param>
        /// <param name="dbContext">Inventory 数据库上下文，用于统一持久化 Inbox 和 Outbox 记录。</param>
        public OrderCreatedIntegrationEventHandler(
            IInventoryApplicationService inventoryService,
            IInboxMessageRepository inboxRepository,
            IOutboxMessageRepository outboxRepository,
            InventoryDbContext dbContext)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _inboxRepository = inboxRepository ?? throw new ArgumentNullException(nameof(inboxRepository));
            _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// 处理订单已创建事件，为订单明细中的每个 SKU 锁定库存，并发出库存已锁定事件。
        /// </summary>
        /// <param name="event">订单已创建集成事件。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>处理任务。</returns>
        public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(@event);

            // 1. 幂等判断：同一个 EventId 已经处理过则直接跳过。
            if (await _inboxRepository.ExistsAsync(@event.EventId, ct))
            {
                return;
            }

            // 2. 遍历订单明细行，逐行锁定库存。
            foreach (var line in @event.Lines)
            {
                var command = new ReserveInventoryCommand(
                    line.SkuId,
                    @event.ExternalOrderNo,
                    line.Quantity);

                await _inventoryService.ReserveInventoryAsync(command, ct);
            }

            // 3. 全部锁定成功后，写入 Inbox 记录作为已处理凭证。
            var inboxPayload = JsonSerializer.Serialize(@event);

            var inboxMessage = new InboxMessage(
                @event.EventId,
                @event.EventType,
                inboxPayload,
                @event.TenantId);

            await _inboxRepository.AddAsync(inboxMessage, ct);

            // 4. 同时写入 Outbox 记录，通知下游履约模块库存已锁定。
            var inventoryReservedEvent = new InventoryReservedIntegrationEvent(
                @event.TenantId,
                @event.AggregateId,
                DateTimeOffset.UtcNow,
                @event.ExternalOrderNo,
                "Reserved");

            var outboxPayload = JsonSerializer.Serialize(inventoryReservedEvent);

            var outboxMessage = new OutboxMessage(
                InventoryReservedIntegrationEvent.TypeName,
                outboxPayload);

            await _outboxRepository.AddAsync(outboxMessage);

            // 5. 持久化 Inbox 和 Outbox 记录（同一事务）。
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
