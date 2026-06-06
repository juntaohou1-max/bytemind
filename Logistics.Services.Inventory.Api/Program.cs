using Logistics.Services.Inventory.Api.Application.IntegrationEvents;
using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Infrastructure.Inbox;
using Logistics.Services.Inventory.Api.Infrastructure.IntegrationEvents;
using Logistics.Services.Inventory.Api.Infrastructure.Outbox;
using Logistics.Services.Inventory.Api.Infrastructure.Persistence;
using Logistics.Services.Inventory.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 注册控制器，后续 Inventory API 接口会通过 Controller 对外暴露。
builder.Services.AddControllers();

// 注册 OpenAPI，便于开发阶段查看和调试接口。
builder.Services.AddOpenApi();
builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb"));
});
// 注册库存总账仓储，后续应用服务通过接口操作库存聚合。
builder.Services.AddScoped<IInventoryItemRepository, EfCoreInventoryItemRepository>();
// 注册 Inventory 应用服务，后续 Controller 通过它调用库存业务用例。
builder.Services.AddScoped<IInventoryApplicationService, InventoryApplicationService>();
builder.Services.AddScoped<IInboxMessageRepository, EfCoreInboxMessageRepository>();
// 注册 Outbox 消息仓储，用于在业务事务中写入待发布事件。
builder.Services.AddScoped<IOutboxMessageRepository, EfCoreOutboxMessageRepository>();
// 注册集成事件发布器，当前为日志实现，后续替换为 RabbitMQ / Kafka。
builder.Services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
// 注册 Outbox 后台发布器，应用启动后自动扫描并发布 Pending 消息。
builder.Services.AddHostedService<OutboxMessagePublisher>();


var app = builder.Build();

// 开发环境下启用 OpenAPI 文档。
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
