using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Tests.Domain
{
    /// <summary>
    /// 库存流水领域规则测试。
    /// </summary>
    public class InventoryTransactionTests
    {
        /// <summary>
        /// 创建库存流水时，传入有效字段应该初始化基础属性。
        /// </summary>
        [Fact]
        public void Create_ShouldSucceed_WhenRequiredFieldsAreProvided()
        {
            var reservationId = Guid.NewGuid();
            var beforeCreate = DateTimeOffset.UtcNow;

            var transaction = new InventoryTransaction(
                " SKU-001 ",
                InventoryTransactionType.Adjustment,
                100,
                reservationId,
                " INITIAL-STOCK ");

            var afterCreate = DateTimeOffset.UtcNow;

            Assert.NotEqual(Guid.Empty, transaction.Id);
            Assert.Equal("SKU-001", transaction.SkuId);
            Assert.Equal(InventoryTransactionType.Adjustment, transaction.Type);
            Assert.Equal(100, transaction.Quantity);
            Assert.Equal(reservationId, transaction.ReservationId);
            Assert.Equal("INITIAL-STOCK", transaction.ReferenceNo);
            Assert.InRange(transaction.CreatedAt, beforeCreate, afterCreate);
        }

        /// <summary>
        /// 创建库存流水时，SKU 为空应该抛出参数异常。
        /// </summary>
        /// <param name="skuId">无效的 SKU 标识。</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_ShouldThrowException_WhenSkuIdIsEmpty(string skuId)
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new InventoryTransaction(
                    skuId,
                    InventoryTransactionType.Adjustment,
                    100));

            Assert.Equal("skuId", exception.ParamName);
        }
    }
}
