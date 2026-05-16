using Logistics.Services.Ordering.Api.Application.Orders;
using Logistics.Services.Ordering.Api.Infrastructure.Persistence;
using Logistics.Services.Ordering.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IOrderRepository, EfCoreOrderRepository>();
builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();
builder.Services.AddScoped<IOutboxMessageRepository, EfCoreOutboxMessageRepository>();
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
