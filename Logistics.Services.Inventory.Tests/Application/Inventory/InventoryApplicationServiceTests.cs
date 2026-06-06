using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Domain;
using Logistics.Services.Inventory.Api.Repositories;

namespace Logistics.Services.Inventory.Tests.Application.Inventory
{
    /// <summary>
    /// Inventory 应用服务测试。
    /// </summary>
    public class InventoryApplicationServiceTests
    {
        /// <summary>
        /// 根据 SKU 查询库存时，SKU 存在应该返回库存总账结果。
        /// </summary>
        [Fact]
        public async Task GetBySkuIdAsync_ShouldReturnInventoryItem_WhenSkuExists()
        {
            var repository = new FakeInventoryItemRepository();
            var item = CreateInventoryItem("SKU-001", 100);
            repository.AddItem(item);
            var service = new InventoryApplicationService(repository);

            var result = await service.GetBySkuIdAsync(" SKU-001 ");

            Assert.NotNull(result);
            Assert.Equal(item.Id, result.Id);
            Assert.Equal("SKU-001", result.SkuId);
            Assert.Equal(100, result.OnHandQuantity);
            Assert.Equal(100, result.AvailableQuantity);
        }

        /// <summary>
        /// 根据 SKU 查询库存时，SKU 不存在应该返回 null。
        /// </summary>
        [Fact]
        public async Task GetBySkuIdAsync_ShouldReturnNull_WhenSkuDoesNotExist()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);

            var result = await service.GetBySkuIdAsync("SKU-404");

            Assert.Null(result);
        }

        /// <summary>
        /// 根据 SKU 查询库存时，SKU 为空应该抛出参数异常。
        /// </summary>
        /// <param name="skuId">无效的 SKU 标识。</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetBySkuIdAsync_ShouldThrowException_WhenSkuIdIsEmpty(string skuId)
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetBySkuIdAsync(skuId));

            Assert.Equal("skuId", exception.ParamName);
        }

        /// <summary>
        /// 调整库存时，SKU 存在应该调整库存并保存变更。
        /// </summary>
        [Fact]
        public async Task AdjustInventoryAsync_ShouldAdjustInventoryAndSaveChanges_WhenSkuExists()
        {
            var repository = new FakeInventoryItemRepository();
            var item = CreateInventoryItem("SKU-001", 100);
            repository.AddItem(item);
            var service = new InventoryApplicationService(repository);
            var command = new AdjustInventoryCommand("SKU-001", -20, "STOCKTAKE-001");

            var result = await service.AdjustInventoryAsync(command);

            Assert.Equal(80, result.OnHandQuantity);
            Assert.Equal(80, result.AvailableQuantity);
            Assert.Equal(80, item.OnHandQuantity);
            Assert.Equal(1, repository.SaveChangesCount);
        }

        /// <summary>
        /// 调整库存时，命令为空应该抛出参数异常。
        /// </summary>
        [Fact]
        public async Task AdjustInventoryAsync_ShouldThrowException_WhenCommandIsNull()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AdjustInventoryAsync(null!));

            Assert.Equal("command", exception.ParamName);
        }

        /// <summary>
        /// 调整库存时，SKU 不存在应该创建库存总账并保存变更。
        /// </summary>
        [Fact]
        public async Task AdjustInventoryAsync_ShouldCreateInventoryItemAndSaveChanges_WhenSkuDoesNotExist()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);
            var command = new AdjustInventoryCommand("SKU-404", 10, "INITIAL-STOCK");

            var result = await service.AdjustInventoryAsync(command);

            Assert.Equal("SKU-404", result.SkuId);
            Assert.Equal(10, result.OnHandQuantity);
            Assert.Equal(10, result.AvailableQuantity);
            Assert.Equal(1, repository.SaveChangesCount);

            var createdItem = await repository.GetBySkuIdAsync("SKU-404");
            Assert.NotNull(createdItem);
            Assert.Equal(10, createdItem.OnHandQuantity);
        }

        /// <summary>
        /// 锁定库存时，SKU 存在且库存足够应该创建预留并保存变更。
        /// </summary>
        [Fact]
        public async Task ReserveInventoryAsync_ShouldReserveInventoryAndSaveChanges_WhenSkuExists()
        {
            var repository = new FakeInventoryItemRepository();
            var item = CreateInventoryItem("SKU-001", 100);
            repository.AddItem(item);
            var service = new InventoryApplicationService(repository);
            var command = new ReserveInventoryCommand("SKU-001", " ERP-001 ", 30);

            var result = await service.ReserveInventoryAsync(command);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("ERP-001", result.ExternalOrderNo);
            Assert.Equal("SKU-001", result.SkuId);
            Assert.Equal(30, result.Quantity);
            Assert.Equal(InventoryReservationStatus.Active.ToString(), result.Status);
            Assert.Equal(30, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);
            Assert.Equal(1, repository.SaveChangesCount);
        }

        /// <summary>
        /// 锁定库存时，SKU 不存在应该抛出未找到异常。
        /// </summary>
        [Fact]
        public async Task ReserveInventoryAsync_ShouldThrowException_WhenSkuDoesNotExist()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);
            var command = new ReserveInventoryCommand("SKU-404", "ERP-001", 30);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ReserveInventoryAsync(command));

            Assert.Equal(0, repository.SaveChangesCount);
        }

        /// <summary>
        /// 释放库存预留时，预留存在应该释放预留并保存变更。
        /// </summary>
        [Fact]
        public async Task ReleaseReservationAsync_ShouldReleaseReservationAndSaveChanges_WhenReservationExists()
        {
            var repository = new FakeInventoryItemRepository();
            var item = CreateInventoryItem("SKU-001", 100);
            var reservation = item.Reserve("ERP-001", 30);
            repository.AddItem(item);
            var service = new InventoryApplicationService(repository);

            await service.ReleaseReservationAsync(reservation.Id);

            Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(100, item.AvailableQuantity);
            Assert.Equal(1, repository.SaveChangesCount);
        }

        /// <summary>
        /// 释放库存预留时，预留不存在应该抛出未找到异常。
        /// </summary>
        [Fact]
        public async Task ReleaseReservationAsync_ShouldThrowException_WhenReservationDoesNotExist()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.ReleaseReservationAsync(Guid.NewGuid()));

            Assert.Equal(0, repository.SaveChangesCount);
        }

        /// <summary>
        /// 扣减库存预留时，预留存在应该扣减预留并保存变更。
        /// </summary>
        [Fact]
        public async Task DeductReservationAsync_ShouldDeductReservationAndSaveChanges_WhenReservationExists()
        {
            var repository = new FakeInventoryItemRepository();
            var item = CreateInventoryItem("SKU-001", 100);
            var reservation = item.Reserve("ERP-001", 30);
            repository.AddItem(item);
            var service = new InventoryApplicationService(repository);

            await service.DeductReservationAsync(reservation.Id);

            Assert.Equal(InventoryReservationStatus.Deducted, reservation.Status);
            Assert.Equal(70, item.OnHandQuantity);
            Assert.Equal(0, item.ReservedQuantity);
            Assert.Equal(70, item.AvailableQuantity);
            Assert.Equal(1, repository.SaveChangesCount);
        }

        /// <summary>
        /// 扣减库存预留时，预留不存在应该抛出未找到异常。
        /// </summary>
        [Fact]
        public async Task DeductReservationAsync_ShouldThrowException_WhenReservationDoesNotExist()
        {
            var repository = new FakeInventoryItemRepository();
            var service = new InventoryApplicationService(repository);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.DeductReservationAsync(Guid.NewGuid()));

            Assert.Equal(0, repository.SaveChangesCount);
        }

        /// <summary>
        /// 创建库存总账测试数据。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        /// <param name="initialQuantity">初始在手库存数量。</param>
        private static InventoryItem CreateInventoryItem(string skuId, int initialQuantity)
        {
            var item = new InventoryItem(skuId);
            item.Adjust(initialQuantity, "INITIAL-STOCK");
            return item;
        }

        /// <summary>
        /// 用于应用服务单元测试的内存库存总账仓储。
        /// </summary>
        private sealed class FakeInventoryItemRepository : IInventoryItemRepository
        {
            private readonly List<InventoryItem> _items = [];

            /// <summary>
            /// 保存变更调用次数。
            /// </summary>
            public int SaveChangesCount { get; private set; }

            /// <summary>
            /// 添加库存总账测试数据。
            /// </summary>
            /// <param name="item">库存总账测试数据。</param>
            public void AddItem(InventoryItem item)
            {
                _items.Add(item);
            }

            /// <summary>
            /// 新增库存总账。
            /// </summary>
            /// <param name="item">要保存的库存总账。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task AddAsync(InventoryItem item, CancellationToken cancellationToken = default)
            {
                _items.Add(item);
                return Task.CompletedTask;
            }

            /// <summary>
            /// 根据库存总账 ID 查询库存总账。
            /// </summary>
            /// <param name="id">库存总账 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
            }

            /// <summary>
            /// 根据 SKU 查询库存总账。
            /// </summary>
            /// <param name="skuId">SKU 标识。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItem?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default)
            {
                var normalizedSkuId = skuId.Trim();
                return Task.FromResult(_items.FirstOrDefault(item => item.SkuId == normalizedSkuId));
            }

            /// <summary>
            /// 根据库存预留 ID 查询所属库存总账。
            /// </summary>
            /// <param name="reservationId">库存预留 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItem?> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_items.FirstOrDefault(item =>
                    item.Reservations.Any(reservation => reservation.Id == reservationId)));
            }

            /// <summary>
            /// 保存已跟踪库存聚合的变更。
            /// </summary>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                SaveChangesCount++;
                return Task.CompletedTask;
            }
        }
    }
}
