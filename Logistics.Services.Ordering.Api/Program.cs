using Logistics.Services.Ordering.Api.Application.IntegrationEvents;
using Logistics.Services.Ordering.Api.Application.Orders;
using Logistics.Services.Ordering.Api.Application.OutboxMessages;
using Logistics.Services.Ordering.Api.Infrastructure.IntegrationEvents;
using Logistics.Services.Ordering.Api.Infrastructure.Outbox;
using Logistics.Services.Ordering.Api.Infrastructure.Persistence;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IOrderRepository, EfCoreOrderRepository>();
builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();
builder.Services.AddScoped<IOutboxMessageRepository, EfCoreOutboxMessageRepository>();
builder.Services.AddScoped<IOutboxMessageQueryService, EfCoreOutboxMessageQueryService>();
builder.Services.AddScoped<IOutboxMessageOperationService, EfCoreOutboxMessageOperationService>();
builder.Services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
// 注册 Outbox 后台发布器，应用启动后自动扫描并处理 Pending 消息。
builder.Services.AddHostedService<OutboxMessagePublisher>();
builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderingDb"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
