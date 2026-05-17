using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Tests.Domain
{
    /// <summary>
    /// 库存预留领域规则测试。
    /// </summary>
    public class InventoryReservationTests
    {
        /// <summary>
        /// 创建库存预留时，传入有效字段应该初始化基础属性。
        /// </summary>
        [Fact]
        public void Create_ShouldSucceed_WhenRequiredFieldsAreProvided()
        {
            var beforeCreate = DateTimeOffset.UtcNow;

            var reservation = new InventoryReservation(
                " ERP-001 ",
                " SKU-001 ",
                10);

            var afterCreate = DateTimeOffset.UtcNow;

            Assert.NotEqual(Guid.Empty, reservation.Id);
            Assert.Equal("ERP-001", reservation.ExternalOrderNo);
            Assert.Equal("SKU-001", reservation.SkuId);
            Assert.Equal(10, reservation.Quantity);
            Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
            Assert.InRange(reservation.CreatedAt, beforeCreate, afterCreate);
        }

        /// <summary>
        /// 创建库存预留时，必填文本为空应该抛出参数异常。
        /// </summary>
        /// <param name="emptyField">为空的字段名。</param>
        [Theory]
        [InlineData("externalOrderNo")]
        [InlineData("skuId")]
        public void Create_ShouldThrowException_WhenRequiredTextFieldIsEmpty(string emptyField)
        {
            var externalOrderNo = "ERP-001";
            var skuId = "SKU-001";

            switch (emptyField)
            {
                case "externalOrderNo":
                    externalOrderNo = "";
                    break;
                case "skuId":
                    skuId = "";
                    break;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new InventoryReservation(externalOrderNo, skuId, 10));

            Assert.Equal(emptyField, exception.ParamName);
        }

        /// <summary>
        /// 创建库存预留时，锁定数量小于等于 0 应该抛出参数范围异常。
        /// </summary>
        /// <param name="quantity">无效的锁定数量。</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_ShouldThrowException_WhenQuantityIsLessThanOrEqualToZero(int quantity)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new InventoryReservation("ERP-001", "SKU-001", quantity));

            Assert.Equal("quantity", exception.ParamName);
        }

        /// <summary>
        /// 释放库存预留时，活跃状态应该变为已释放。
        /// </summary>
        [Fact]
        public void Release_ShouldChangeStatusToReleased_WhenReservationIsActive()
        {
            var reservation = CreateReservation();

            reservation.Release();

            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        }

        /// <summary>
        /// 扣减库存预留时，活跃状态应该变为已扣减。
        /// </summary>
        [Fact]
        public void Deduct_ShouldChangeStatusToDeducted_WhenReservationIsActive()
        {
            var reservation = CreateReservation();

            reservation.Deduct();

            Assert.Equal(InventoryReservationStatus.Deducted, reservation.Status);
        }

        /// <summary>
        /// 释放库存预留时，非活跃状态应该抛出业务异常。
        /// </summary>
        [Fact]
        public void Release_ShouldThrowException_WhenReservationIsNotActive()
        {
            var reservation = CreateReservation();
            reservation.Deduct();

            Assert.Throws<InvalidOperationException>(() => reservation.Release());
            Assert.Equal(InventoryReservationStatus.Deducted, reservation.Status);
        }

        /// <summary>
        /// 扣减库存预留时，非活跃状态应该抛出业务异常。
        /// </summary>
        [Fact]
        public void Deduct_ShouldThrowException_WhenReservationIsNotActive()
        {
            var reservation = CreateReservation();
            reservation.Release();

            Assert.Throws<InvalidOperationException>(() => reservation.Deduct());
            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        }

        /// <summary>
        /// 创建一条有效的库存预留测试数据。
        /// </summary>
        private static InventoryReservation CreateReservation()
        {
            return new InventoryReservation("ERP-001", "SKU-001", 10);
        }
    }
}
