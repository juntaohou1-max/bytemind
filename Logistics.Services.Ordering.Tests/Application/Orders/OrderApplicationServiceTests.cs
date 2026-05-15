using Logistics.Services.Ordering.Api.Application.Orders;
using Logistics.Services.Ordering.Api.Contracts.Orders;
using Logistics.Services.Ordering.Api.Domain;
using Logistics.Services.Ordering.Api.Repositories;

namespace Logistics.Services.Ordering.Tests.Application.Orders
{
    public class OrderApplicationServiceTests
    {
        [Fact]
        public void Create_ShouldSaveOrderAndReturnOrderId()
        {
            var repository = new InMemoryOrderRepository();
            var service = new OrderApplicationService(repository);

            var response = service.Create(CreateRequest("ERP001"));

            var savedOrder = repository.GetById(response.Id);
            Assert.NotNull(savedOrder);
            Assert.Equal("ERP001", savedOrder.ExternalOrderNo);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var response = service.GetById(Guid.NewGuid());

            Assert.Null(response);
        }

        [Fact]
        public void GetById_ShouldReturnOrderDetail_WhenOrderExists()
        {
            var repository = new InMemoryOrderRepository();
            var order = CreateOrder("ERP001");
            repository.Add(order);
            var service = new OrderApplicationService(repository);

            var response = service.GetById(order.Id);

            Assert.NotNull(response);
            Assert.Equal(order.Id, response.Id);
            Assert.Equal("ERP001", response.ExternalOrderNo);
            Assert.Equal("Created", response.Status);
        }

        [Fact]
        public void Cancel_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var cancelled = service.Cancel(Guid.NewGuid());

            Assert.False(cancelled);
        }

        [Fact]
        public void Cancel_ShouldCancelOrder_WhenOrderExists()
        {
            var repository = new InMemoryOrderRepository();
            var order = CreateOrder("ERP001");
            repository.Add(order);
            var service = new OrderApplicationService(repository);

            var cancelled = service.Cancel(order.Id);

            Assert.True(cancelled);
            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public void GetAll_ShouldFilterOrdersByStatus()
        {
            var repository = new InMemoryOrderRepository();
            var createdOrder = CreateOrder("ERP001");
            var cancelledOrder = CreateOrder("ERP002");
            cancelledOrder.Cancel();
            repository.Add(createdOrder);
            repository.Add(cancelledOrder);
            var service = new OrderApplicationService(repository);

            var response = service.GetAll("Cancelled", null, null, null);

            var order = Assert.Single(response);
            Assert.Equal(cancelledOrder.Id, order.Id);
            Assert.Equal("Cancelled", order.Status);
        }

        [Fact]
        public void GetAll_ShouldFilterOrdersByExternalOrderNo()
        {
            var repository = new InMemoryOrderRepository();
            var targetOrder = CreateOrder("ERP001");
            repository.Add(targetOrder);
            repository.Add(CreateOrder("ERP002"));
            var service = new OrderApplicationService(repository);

            var response = service.GetAll(null, null, null, "ERP001");

            var order = Assert.Single(response);
            Assert.Equal(targetOrder.Id, order.Id);
            Assert.Equal("ERP001", order.ExternalOrderNo);
        }

        [Fact]
        public void GetAll_ShouldReturnListItemResponse()
        {
            var repository = new InMemoryOrderRepository();
            repository.Add(CreateOrder("ERP001"));
            var service = new OrderApplicationService(repository);

            var response = service.GetAll(null, null, null, null);

            var order = Assert.Single(response);
            Assert.Equal("ERP001", order.ExternalOrderNo);
            Assert.Equal(1, order.LineCount);
        }

        [Fact]
        public void GetAll_ShouldThrowException_WhenStatusIsInvalid()
        {
            var service = new OrderApplicationService(new InMemoryOrderRepository());

            var exception = Assert.Throws<ArgumentException>(() =>
                service.GetAll("Unknown", null, null, null));

            Assert.Equal("status", exception.ParamName);
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
