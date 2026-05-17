var builder = WebApplication.CreateBuilder(args);

// 注册控制器，后续 Inventory API 接口会通过 Controller 对外暴露。
builder.Services.AddControllers();

// 注册 OpenAPI，便于开发阶段查看和调试接口。
builder.Services.AddOpenApi();

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
