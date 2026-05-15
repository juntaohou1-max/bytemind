using Logistics.Services.Ordering.Api.Application.Orders;
using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;

namespace Logistics.Services.Ordering.Tests.Application.Orders
{
    public class OrderApplicationServiceTests
    {
        [Fact]
        public async Task Create_ShouldSaveOrderAndReturnOrderId()
        {
            var repository = new InMemoryOrderRepository();
            var service = new OrderApplicationService(repository);

            var response = await service.CreateAsync(CreateRequest("ERP001"));

            var savedOrder = await repository.GetByIdAsync(response.Id);
            Assert.NotNull(savedOrder);
            Assert.Equal("ERP001", savedOrder.ExternalOrderNo);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var response = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(response);
        }

        [Fact]
        public async Task GetById_ShouldReturnOrderDetail_WhenOrderExists()
        {
            var repository = new InMemoryOrderRepository();
            var order = CreateOrder("ERP001");
            await repository.AddAsync(order);
            var service = new OrderApplicationService(repository);

            var response = await service.GetByIdAsync(order.Id);

            Assert.NotNull(response);
            Assert.Equal(order.Id, response.Id);
            Assert.Equal("ERP001", response.ExternalOrderNo);
            Assert.Equal("Created", response.Status);
        }

        [Fact]
        public async Task Cancel_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var cancelled = await service.CancelAsync(Guid.NewGuid());

            Assert.False(cancelled);
        }

        [Fact]
        public async Task Cancel_ShouldCancelOrder_WhenOrderExists()
        {
            var repository = new InMemoryOrderRepository();
            var order = CreateOrder("ERP001");
            await repository.AddAsync(order);
            var service = new OrderApplicationService(repository);

            var cancelled = await service.CancelAsync(order.Id);

            Assert.True(cancelled);
            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public async Task GetAll_ShouldFilterOrdersByStatus()
        {
            var repository = new InMemoryOrderRepository();
            var createdOrder = CreateOrder("ERP001");
            var cancelledOrder = CreateOrder("ERP002");
            cancelledOrder.Cancel();
            await repository.AddAsync(createdOrder);
            await repository.AddAsync(cancelledOrder);
            var service = new OrderApplicationService(repository);

            var response = await service.GetAllAsync("Cancelled", null, null, null, 1, 20, "createdAtDesc");

            var order = Assert.Single(response.Items);
            Assert.Equal(cancelledOrder.Id, order.Id);
            Assert.Equal("Cancelled", order.Status);
            Assert.Equal(1, response.TotalCount);
        }

        [Fact]
        public async Task GetAll_ShouldFilterOrdersByExternalOrderNo()
        {
            var repository = new InMemoryOrderRepository();
            var targetOrder = CreateOrder("ERP001");
            await repository.AddAsync(targetOrder);
            await repository.AddAsync(CreateOrder("ERP002"));
            var service = new OrderApplicationService(repository);

            var response = await service.GetAllAsync(null, null, null, "ERP001", 1, 20, "createdAtDesc");

            var order = Assert.Single(response.Items);
            Assert.Equal(targetOrder.Id, order.Id);
            Assert.Equal("ERP001", order.ExternalOrderNo);
            Assert.Equal(1, response.TotalCount);
        }

        [Fact]
        public async Task GetAll_ShouldReturnListItemResponse()
        {
            var repository = new InMemoryOrderRepository();
            await repository.AddAsync(CreateOrder("ERP001"));
            var service = new OrderApplicationService(repository);

            var response = await service.GetAllAsync(null, null, null, null, 1, 20, "createdAtDesc");

            var order = Assert.Single(response.Items);
            Assert.Equal("ERP001", order.ExternalOrderNo);
            Assert.Equal(1, order.LineCount);
            Assert.Equal(1, response.PageNumber);
            Assert.Equal(20, response.PageSize);
            Assert.Equal(1, response.TotalCount);
        }

        [Fact]
        public async Task GetAll_ShouldThrowException_WhenStatusIsInvalid()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetAllAsync("Unknown", null, null, null, 1, 20, "createdAtDesc"));

            Assert.Equal("status", exception.ParamName);
        }

        [Fact]
        public async Task GetAll_ShouldReturnRequestedPageAndTotalCount()
        {
            var repository = new InMemoryOrderRepository();
            var firstOrder = CreateOrder("ERP001");
            await Task.Delay(5);
            var secondOrder = CreateOrder("ERP002");
            await Task.Delay(5);
            var thirdOrder = CreateOrder("ERP003");
            await repository.AddAsync(firstOrder);
            await repository.AddAsync(secondOrder);
            await repository.AddAsync(thirdOrder);
            var service = new OrderApplicationService(repository);

            var response = await service.GetAllAsync(null, null, null, null, 2, 1, "createdAtAsc");

            var order = Assert.Single(response.Items);
            Assert.Equal("ERP002", order.ExternalOrderNo);
            Assert.Equal(2, response.PageNumber);
            Assert.Equal(1, response.PageSize);
            Assert.Equal(3, response.TotalCount);
            Assert.Equal(3, response.TotalPages);
        }

        private static Order CreateOrder(string externalOrderNo)
        {
            return new Order(
                "tenant-001",
                "customer-001",
                externalOrderNo,
                CreateAddress(),
                new[] { new OrderLine("SKU-001", 2) });
        }

        private static CreateOrderRequest CreateRequest(string externalOrderNo)
        {
            return new CreateOrderRequest
            {
                TenantId = "tenant-001",
                CustomerId = "customer-001",
                ExternalOrderNo = externalOrderNo,
                ReceiverAddress = new AddressRequest
                {
                    ReceiverName = "张三",
                    Phone = "13800000000",
                    Province = "浙江省",
                    City = "杭州市",
                    District = "西湖区",
                    Detail = "文三路 100 号"
                },
                Lines =
                [
                    new CreateOrderLineRequest
                    {
                        SkuId = "SKU-001",
                        Quantity = 2
                    }
                ]
            };
        }

        private static Address CreateAddress()
        {
            return new Address(
                "张三",
                "13800000000",
                "浙江省",
                "杭州市",
                "西湖区",
                "文三路 100 号");
        }
    }
}
