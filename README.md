# LogisticsPlatform

物流履约 / 仓储调度系统学习项目。

当前阶段只实现第一个 ASP.NET Core Web API 项目：`Logistics.Services.Ordering.Api`。

## 项目定位

本项目不是商城系统。这里的 `Order` 表示外部系统发送给仓储系统的一张发货指令。

Ordering 模块第一阶段只负责：

- 接收订单
- 保存订单
- 查询订单
- 取消订单
- 查看订单时间线

暂时不处理库存、拣货、打包、运输和数据库持久化。

## 当前技术栈

- .NET 9
- ASP.NET Core Web API
- xUnit
- 内存仓储 `InMemoryOrderRepository`

## 当前项目结构

```text
LogisticsPlatform
├── Logistics.Services.Ordering.Api
│   ├── Application
│   ├── Contracts
│   ├── Controllers
│   ├── Domain
│   └── Repositories
├── Logistics.Services.Ordering.Tests
└── LogisticsPlatform.sln
```

## 已实现接口

```http
POST /api/orders
GET /api/orders
GET /api/orders/{id}
POST /api/orders/{id}/cancel
GET /api/orders/{id}/timeline
```

订单列表支持基础筛选：

```http
GET /api/orders?status=Created
GET /api/orders?status=Cancelled
GET /api/orders?from=2026-05-14T00:00:00Z
GET /api/orders?to=2026-05-15T00:00:00Z
GET /api/orders?externalOrderNo=ERP202605120001
```

## 领域模型

当前领域模型包括：

- `Order`：发货订单聚合根
- `OrderLine`：订单明细
- `Address`：收货地址值对象
- `OrderStatus`：订单状态
- `OrderTimelineItem`：订单时间线记录

当前已覆盖的核心规则：

- 订单必须有外部订单号
- 订单必须有收货地址
- 订单至少有一条明细
- 订单明细数量必须大于 0
- 订单取消会记录时间线
- 订单创建会记录时间线

## 运行项目

在仓库根目录执行：

```powershell
dotnet run --project .\Logistics.Services.Ordering.Api\
```

默认 HTTP 地址：

```text
http://localhost:5299
```

也可以使用 Visual Studio 启动 `Logistics.Services.Ordering.Api`。

## 手动测试

项目内提供 `.http` 文件：

```text
Logistics.Services.Ordering.Api/Logistics.Services.Ordering.Api.http
```

可以在 Visual Studio 中直接发送请求。

## 运行测试

在仓库根目录执行：

```powershell
dotnet test
```

当前测试覆盖：

- `OrderLine` 规则
- `Address` 规则
- `Order` 聚合根规则
- `OrderApplicationService` 基础用例

## 后续计划

- 完善订单状态流转
- 增加应用服务层用例
- 接入 EF Core 和数据库持久化
- 引入库存模块
- 引入履约模块
- 后续再考虑事件总线、Outbox/Inbox、Docker 和认证授权
