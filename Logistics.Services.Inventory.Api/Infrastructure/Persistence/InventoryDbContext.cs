using Logistics.Services.Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Services.Inventory.Api.Infrastructure.Persistence
{
    public class InventoryDbContext : DbContext
    {
        /// <summary>
        /// 创建 Inventory 数据库上下文。
        /// </summary>
        /// <param name="options">数据库上下文配置。</param>
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
         : base(options)
        {
        }

        /// <summary>
        /// SKU 库存总账集合。
        /// </summary>
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

        /// <summary>
        /// 库存预留集合。
        /// </summary>
        public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();

        /// <summary>
        /// 库存流水集合。
        /// </summary>
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();


        /// <summary>
        /// 配置 Inventory 模块领域模型到数据库表的映射。
        /// </summary>
        /// <param name="modelBuilder">模型构建器。</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureInventoryItem(modelBuilder.Entity<InventoryItem>());
            ConfigureInventoryReservation(modelBuilder.Entity<InventoryReservation>());
            ConfigureInventoryTransaction(modelBuilder.Entity<InventoryTransaction>());
        }


        /// <summary>
        /// 配置库存总账表映射。
        /// </summary>
        /// <param name="builder">库存总账实体配置器。</param>
        private static void ConfigureInventoryItem(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(item => item.Id);
            builder.Property(item => item.SkuId)
                 .HasMaxLength(100)
                .IsRequired();

            //给 SkuId 建一个唯一索引。InventoryItems 表里的 SkuId 不能重复
            builder.HasIndex(item => item.SkuId)
             .IsUnique();

            builder.Property(item => item.OnHandQuantity)
                .IsRequired();

            builder.Property(item => item.ReservedQuantity)
                .IsRequired();

            builder.Property(item => item.DamagedQuantity)
                .IsRequired();

            builder.Ignore(item => item.AvailableQuantity);

            builder.HasMany(item => item.Reservations)
                .WithOne()
                .HasForeignKey("InventoryItemId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(item => item.Transactions)
                .WithOne()
                .HasForeignKey("InventoryItemId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(item => item.Reservations)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(item => item.Transactions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
        /// <summary>
        /// 配置库存预留表映射。
        /// </summary>
        /// <param name="builder">库存预留实体配置器。</param>
        private static void ConfigureInventoryReservation(EntityTypeBuilder<InventoryReservation> builder)
        {
            builder.ToTable("InventoryReservations");

            builder.HasKey(reservation => reservation.Id);

            builder.Property<Guid>("InventoryItemId")
                .IsRequired();

            builder.Property(reservation => reservation.ExternalOrderNo)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(reservation => reservation.SkuId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(reservation => reservation.Quantity)
                .IsRequired();

            builder.Property(reservation => reservation.Status)
                .IsRequired();

            builder.Property(reservation => reservation.CreatedAt)
                .IsRequired();

            builder.HasIndex("InventoryItemId");

            builder.HasIndex(reservation => reservation.ExternalOrderNo);
        }

        /// <summary>
        /// 配置库存流水表映射。
        /// </summary>
        /// <param name="builder">库存流水实体配置器。</param>
        private static void ConfigureInventoryTransaction(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(transaction => transaction.Id);

            builder.Property<Guid>("InventoryItemId")
                .IsRequired();

            builder.Property(transaction => transaction.SkuId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(transaction => transaction.Type)
                .IsRequired();

            builder.Property(transaction => transaction.Quantity)
                .IsRequired();

            builder.Property(transaction => transaction.ReservationId);

            builder.Property(transaction => transaction.ReferenceNo)
                .HasMaxLength(100);

            builder.Property(transaction => transaction.CreatedAt)
                .IsRequired();

            builder.HasIndex("InventoryItemId");

            builder.HasIndex(transaction => transaction.SkuId);

            builder.HasIndex(transaction => transaction.ReservationId);
        }
    }
}
