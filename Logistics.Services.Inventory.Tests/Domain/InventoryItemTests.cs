using Logistics.Services.Inventory.Api.Domain;

namespace Logistics.Services.Inventory.Tests.Domain
{
    /// <summary>
    /// 库存总账领域规则测试。
    /// </summary>
    public class InventoryItemTests
    {
        /// <summary>
        /// 创建库存总账时，传入有效 SKU 应该初始化基础字段。
        /// </summary>
        [Fact]
        public void Create_ShouldSucceed_WhenSkuIdIsProvided()
        {
            var item = new InventoryItem(" SKU-001 ");

            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.Equal("SKU-001", item.SkuId);
            Assert.Equal(0, item.OnHandQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(0, item.DamagedQuantity);
            Assert.Equal(0, item.AvailableQuantity);
            Assert.Empty(item.Reservations);
            Assert.Empty(item.Transactions);
        }

        /// <summary>
        /// 创建库存总账时，SKU 为空应该抛出参数异常。
        /// </summary>
        /// <param name="skuId">无效的 SKU 标识。</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_ShouldThrowException_WhenSkuIdIsEmpty(string skuId)
        {
            var exception = Assert.Throws<ArgumentException>(() => new InventoryItem(skuId));

            Assert.Equal("skuId", exception.ParamName);
        }

        /// <summary>
        /// 调整库存时，应该修改在手库存并生成库存流水。
        /// </summary>
        [Fact]
        public void Adjust_ShouldIncreaseOnHandQuantityAndCreateTransaction()
        {
            var item = new InventoryItem("SKU-001");

            item.Adjust(100, " INITIAL-STOCK ");

            Assert.Equal(100, item.OnHandQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(0, item.DamagedQuantity);

            var transaction = Assert.Single(item.Transactions);
            Assert.NotEqual(Guid.Empty, transaction.Id);
            Assert.Equal("SKU-001", transaction.SkuId);
            Assert.Equal(InventoryTransactionType.Adjustment, transaction.Type);
            Assert.Equal(100, transaction.Quantity);
            Assert.Null(transaction.ReservationId);
            Assert.Equal("INITIAL-STOCK", transaction.ReferenceNo);
        }

        /// <summary>
        /// 调整库存时，调整数量为 0 应该抛出参数范围异常。
        /// </summary>
        [Fact]
        public void Adjust_ShouldThrowException_WhenQuantityDeltaIsZero()
        {
            var item = new InventoryItem("SKU-001");

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => item.Adjust(0));

            Assert.Equal("quantityDelta", exception.ParamName);
            Assert.Equal(0, item.OnHandQuantity);
            Assert.Empty(item.Transactions);
        }

        /// <summary>
        /// 调整库存时，调整后的在手库存为负数应该抛出业务异常。
        /// </summary>
        [Fact]
        public void Adjust_ShouldThrowException_WhenAdjustedOnHandQuantityWouldBeNegative()
        {
            var item = new InventoryItem("SKU-001");

            Assert.Throws<InvalidOperationException>(() => item.Adjust(-1));

            Assert.Equal(0, item.OnHandQuantity);
            Assert.Empty(item.Transactions);
        }

        /// <summary>
        /// 锁定库存时，可用库存足够应该创建库存预留并生成库存流水。
        /// </summary>
        [Fact]
        public void Reserve_ShouldCreateReservationAndTransaction_WhenAvailableQuantityIsEnough()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");

            var reservation = item.Reserve(" ERP-001 ", 30);

            Assert.NotEqual(Guid.Empty, reservation.Id);
            Assert.Equal("ERP-001", reservation.ExternalOrderNo);
            Assert.Equal("SKU-001", reservation.SkuId);
            Assert.Equal(30, reservation.Quantity);
            Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
            Assert.Equal(100, item.OnHandQuantity);
            Assert.Equal(30, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);
            Assert.Same(reservation, Assert.Single(item.Reservations));

            Assert.Equal(2, item.Transactions.Count);
            var transaction = item.Transactions.Last();
            Assert.Equal("SKU-001", transaction.SkuId);
            Assert.Equal(InventoryTransactionType.ReservationCreated, transaction.Type);
            Assert.Equal(30, transaction.Quantity);
            Assert.Equal(reservation.Id, transaction.ReservationId);
            Assert.Equal("ERP-001", transaction.ReferenceNo);
        }

        /// <summary>
        /// 锁定库存时，锁定数量小于等于 0 应该抛出参数范围异常。
        /// </summary>
        /// <param name="quantity">无效的锁定数量。</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Reserve_ShouldThrowException_WhenQuantityIsLessThanOrEqualToZero(int quantity)
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                item.Reserve("ERP-001", quantity));

            Assert.Equal("quantity", exception.ParamName);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Empty(item.Reservations);
            Assert.Single(item.Transactions);
        }

        /// <summary>
        /// 锁定库存时，可用库存不足应该抛出业务异常。
        /// </summary>
        [Fact]
        public void Reserve_ShouldThrowException_WhenAvailableQuantityIsNotEnough()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");

            Assert.Throws<InvalidOperationException>(() => item.Reserve("ERP-001", 101));

            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Empty(item.Reservations);
            Assert.Single(item.Transactions);
        }

        /// <summary>
        /// 释放库存预留时，活跃预留应该变为已释放并生成库存流水。
        /// </summary>
        [Fact]
        public void ReleaseReservation_ShouldReleaseReservationAndCreateTransaction_WhenReservationIsActive()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);

            item.ReleaseReservation(reservation.Id);

            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
            Assert.Equal(100, item.OnHandQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);

            Assert.Equal(3, item.Transactions.Count);
            var transaction = item.Transactions.Last();
            Assert.Equal("SKU-001", transaction.SkuId);
            Assert.Equal(InventoryTransactionType.ReservationReleased, transaction.Type);
            Assert.Equal(-30, transaction.Quantity);
            Assert.Equal(reservation.Id, transaction.ReservationId);
            Assert.Equal("ERP-001", transaction.ReferenceNo);
        }

        /// <summary>
        /// 释放库存预留时，预留标识不存在应该抛出参数范围异常。
        /// </summary>
        [Fact]
        public void ReleaseReservation_ShouldThrowException_WhenReservationIdDoesNotExist()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                item.ReleaseReservation(Guid.NewGuid()));

            Assert.Equal("reservationId", exception.ParamName);
            Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
            Assert.Equal(30, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);
            Assert.Equal(2, item.Transactions.Count);
        }

        /// <summary>
        /// 释放库存预留时，非活跃预留应该抛出业务异常。
        /// </summary>
        [Fact]
        public void ReleaseReservation_ShouldThrowException_WhenReservationIsNotActive()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);
            item.ReleaseReservation(reservation.Id);

            Assert.Throws<InvalidOperationException>(() => item.ReleaseReservation(reservation.Id));

            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Equal(3, item.Transactions.Count);
        }

        /// <summary>
        /// 扣减库存预留时，活跃预留应该变为已扣减并生成库存流水。
        /// </summary>
        [Fact]
        public void DeductReservation_ShouldDeductReservationAndCreateTransaction_WhenReservationIsActive()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);

            item.DeductReservation(reservation.Id);

            Assert.Equal(InventoryReservationStatus.Deducted, reservation.Status);
            Assert.Equal(70, item.OnHandQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);

            Assert.Equal(3, item.Transactions.Count);
            var transaction = item.Transactions.Last();
            Assert.Equal("SKU-001", transaction.SkuId);
            Assert.Equal(InventoryTransactionType.ReservationDeducted, transaction.Type);
            Assert.Equal(-30, transaction.Quantity);
            Assert.Equal(reservation.Id, transaction.ReservationId);
            Assert.Equal("ERP-001", transaction.ReferenceNo);
        }

        /// <summary>
        /// 扣减库存预留时，预留标识不存在应该抛出参数范围异常。
        /// </summary>
        [Fact]
        public void DeductReservation_ShouldThrowException_WhenReservationIdDoesNotExist()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                item.DeductReservation(Guid.NewGuid()));

            Assert.Equal("reservationId", exception.ParamName);
            Assert.Equal(InventoryReservationStatus.Active, reservation.Status);
            Assert.Equal(100, item.OnHandQuantity);
            Assert.Equal(30, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);
            Assert.Equal(2, item.Transactions.Count);
        }

        /// <summary>
        /// 扣减库存预留时，非活跃预留应该抛出业务异常。
        /// </summary>
        [Fact]
        public void DeductReservation_ShouldThrowException_WhenReservationIsNotActive()
        {
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);
            item.ReleaseReservation(reservation.Id);

            Assert.Throws<InvalidOperationException>(() => item.DeductReservation(reservation.Id));

            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
            Assert.Equal(100, item.OnHandQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Equal(3, item.Transactions.Count);
        }
    }
}
