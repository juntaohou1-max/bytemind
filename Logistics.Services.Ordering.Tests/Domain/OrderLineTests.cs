using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Tests.Domain
{
    public class OrderLineTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenSkuIdAndQuantityAreValid()
        {
            var orderLine = new OrderLine(" SKU-001 ", 2);

            Assert.Equal("SKU-001", orderLine.SkuId);
            Assert.Equal(2, orderLine.Quantity);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenQuantityIsZero()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new OrderLine("SKU-001", 0));

            Assert.Equal("quantity", exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenQuantityIsNegative()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new OrderLine("SKU-001", -1));

            Assert.Equal("quantity", exception.ParamName);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenSkuIdIsEmpty()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new OrderLine("", 1));

            Assert.Equal("skuId", exception.ParamName);
        }
    }
}
