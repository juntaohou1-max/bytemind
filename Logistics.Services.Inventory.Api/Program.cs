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
