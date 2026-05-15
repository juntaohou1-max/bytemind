using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Tests.Domain
{
    public class OrderTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenRequiredFieldsAreProvided()
        {
            var address = CreateAddress();
            var lines = new[] { new OrderLine("SKU-001", 2) };

            var beforeCreate = DateTimeOffset.UtcNow;
            var order = new Order(" tenant-001 ", " customer-001 ", " ERP001 ", address, lines);
            var afterCreate = DateTimeOffset.UtcNow;

            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.Equal("tenant-001", order.TenantId);
            Assert.Equal("customer-001", order.CustomerId);
            Assert.Equal("ERP001", order.ExternalOrderNo);
            Assert.Same(address, order.ReceiverAddress);
            Assert.Single(order.Lines);
            Assert.Equal(OrderStatus.Created, order.Status);
            Assert.InRange(order.CreatedAt, beforeCreate, afterCreate);
        }

        [Theory]
        [InlineData("tenantId")]
        [InlineData("customerId")]
        [InlineData("externalOrderNo")]
        public void Create_ShouldThrowException_WhenRequiredTextFieldIsEmpty(string emptyField)
        {
            var tenantId = "tenant-001";
            var customerId = "customer-001";
            var externalOrderNo = "ERP001";

            switch (emptyField)
            {
                case "tenantId":
                    tenantId = "";
                    break;
                case "customerId":
                    customerId = "";
                    break;
                case "externalOrderNo":
                    externalOrderNo = "";
                    break;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new Order(
                    tenantId,
                    customerId,
                    externalOrderNo,
                    CreateAddress(),
                    CreateLines()));

            Assert.Equal(emptyField, exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenReceiverAddressIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new Order(
                    "tenant-001",
                    "customer-001",
                    "ERP001",
                    null!,
                    CreateLines()));

            Assert.Equal("receiverAddress", exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenLinesIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new Order(
                    "tenant-001",
                    "customer-001",
                    "ERP001",
                    CreateAddress(),
                    null!));

            Assert.Equal("lines", exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenLinesIsEmpty()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new Order(
                    "tenant-001",
                    "customer-001",
                    "ERP001",
                    CreateAddress(),
                    Array.Empty<OrderLine>()));

            Assert.Equal("lines", exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenLinesContainsNull()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new Order(
                    "tenant-001",
                    "customer-001",
                    "ERP001",
                    CreateAddress(),
                    new OrderLine[] { null! }));

            Assert.Equal("lines", exception.ParamName);
        }

        [Fact]
        public void Cancel_ShouldChangeStatusToCancelled()
        {
            var order = CreateOrder();

            order.Cancel();

            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        [Fact]
        public void Cancel_ShouldKeepStatusCancelled_WhenOrderIsAlreadyCancelled()
        {
            var order = CreateOrder();

            order.Cancel();
            order.Cancel();

            Assert.Equal(OrderStatus.Cancelled, order.Status);
        }

        private static Order CreateOrder()
        {
            return new Order(
                "tenant-001",
                "customer-001",
                "ERP001",
                CreateAddress(),
                CreateLines());
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

        private static IReadOnlyCollection<OrderLine> CreateLines()
        {
            return new[]
            {
                new OrderLine("SKU-001", 2)
            };
        }
    }
}
