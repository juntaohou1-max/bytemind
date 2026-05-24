using Logistics.Services.Inventory.Api.Domain;
using Logistics.Services.Inventory.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Inventory.Tests.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core 库存总账仓储测试。
    /// </summary>
    public class EfCoreInventoryItemRepositoryTests
    {
        /// <summary>
        /// 新增库存总账并保存时，应该持久化库存总账基础字段。
        /// </summary>
        [Fact]
        public async Task AddAsync_ShouldSaveInventoryItem()
        {
            var options = CreateOptions();
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");

            await using (var dbContext = new InventoryDbContext(options))
            {
                var repository = new EfCoreInventoryItemRepository(dbContext);

                await repository.AddAsync(item);
                await repository.SaveChangesAsync();
            }

            await using (var dbContext = new InventoryDbContext(options))
            {
                var savedItem = await dbContext.InventoryItems.SingleAsync();

                Assert.Equal("SKU-001", savedItem.SkuId);
                Assert.Equal(100, savedItem.OnHandQuantity);
                Assert.Equal(0, savedItem.ReservedQuantity);
                Assert.Equal(100, savedItem.AvailableQuantity);
            }
        }

        /// <summary>
        /// 根据 SKU 查询库存总账时，应该使用去除首尾空白后的 SKU。
        /// </summary>
        [Fact]
        public async Task GetBySkuIdAsync_ShouldReturnInventoryItem_WhenSkuIdExists()
        {
            var options = CreateOptions();
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            await SeedAsync(options, item);

            await using var dbContext = new InventoryDbContext(options);
            var repository = new EfCoreInventoryItemRepository(dbContext);

            var result = await repository.GetBySkuIdAsync(" SKU-001 ");

            Assert.NotNull(result);
            Assert.Equal(item.Id, result.Id);
            Assert.Equal("SKU-001", result.SkuId);
        }

        /// <summary>
        /// 根据 SKU 查询库存总账时，SKU 为空应该抛出参数异常。
        /// </summary>
        /// <param name="skuId">无效的 SKU 标识。</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetBySkuIdAsync_ShouldThrowException_WhenSkuIdIsEmpty(string skuId)
        {
            var options = CreateOptions();
            await using var dbContext = new InventoryDbContext(options);
            var repository = new EfCoreInventoryItemRepository(dbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                repository.GetBySkuIdAsync(skuId));

            Assert.Equal("skuId", exception.ParamName);
        }

        /// <summary>
        /// 根据库存预留 ID 查询库存总账时，应该加载预留集合以支持后续领域操作。
        /// </summary>
        [Fact]
        public async Task GetByReservationIdAsync_ShouldReturnInventoryItemWithReservations_WhenReservationExists()
        {
            var options = CreateOptions();
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            var reservation = item.Reserve("ERP-001", 30);
            await SeedAsync(options, item);

            await using var dbContext = new InventoryDbContext(options);
            var repository = new EfCoreInventoryItemRepository(dbContext);

            var result = await repository.GetByReservationIdAsync(reservation.Id);

            Assert.NotNull(result);
            Assert.Equal(item.Id, result.Id);
            Assert.Single(result.Reservations);

            result.ReleaseReservation(reservation.Id);

            Assert.Equal(0, result.ReservedQuantity);
            Assert.Equal(100, result.AvailableQuantity);
            Assert.Equal(InventoryReservationStatus.Released, result.Reservations.Single().Status);
        }

        /// <summary>
        /// 根据库存预留 ID 查询库存总账时，预留不存在应该返回 null。
        /// </summary>
        [Fact]
        public async Task GetByReservationIdAsync_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            var options = CreateOptions();
            var item = new InventoryItem("SKU-001");
            item.Adjust(100, "INITIAL-STOCK");
            await SeedAsync(options, item);

            await using var dbContext = new InventoryDbContext(options);
            var repository = new EfCoreInventoryItemRepository(dbContext);

            var result = await repository.GetByReservationIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        /// <summary>
        /// 创建测试用 Inventory 数据库上下文配置。
        /// </summary>
        private static DbContextOptions<InventoryDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        /// <summary>
        /// 写入一条库存总账测试数据。
        /// </summary>
        /// <param name="options">数据库上下文配置。</param>
        /// <param name="item">库存总账测试数据。</param>
        private static async Task SeedAsync(DbContextOptions<InventoryDbContext> options, InventoryItem item)
        {
            await using var dbContext = new InventoryDbContext(options);
            dbContext.InventoryItems.Add(item);
            await dbContext.SaveChangesAsync();
        }
    }
}
