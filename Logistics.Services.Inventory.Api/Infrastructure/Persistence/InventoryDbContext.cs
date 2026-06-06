using Logistics.Services.Inventory.Api.Domain;
using Logistics.Services.Inventory.Api.Domain.Inbox;
using Logistics.Services.Inventory.Api.Domain.Outbox;
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
        /// Inbox 消息集合，用于记录已处理的集成事件，实现幂等消费。
        /// </summary>
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        /// <summary>
        /// Outbox 消息集合，用于保存待发布的集成事件，后续由后台任务负责发送。
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();


        /// <summary>
        /// 配置 Inventory 模块领域模型到数据库表的映射。
        /// </summary>
        /// <param name="modelBuilder">模型构建器。</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureInventoryItem(modelBuilder.Entity<InventoryItem>());
            ConfigureInventoryReservation(modelBuilder.Entity<InventoryReservation>());
            ConfigureInventoryTransaction(modelBuilder.Entity<InventoryTransaction>());
            ConfigureInboxMessage(modelBuilder.Entity<InboxMessage>());
            ConfigureOutboxMessage(modelBuilder.Entity<OutboxMessage>());
        }


        /// <summary>
        /// 配置库存总账表映射。
        /// </summary>
        /// <param name="builder">库存总账实体配置器。</param>
        private static void ConfigureInventoryItem(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(item => item.Id);
            builder.Property(item => item.Id)
                .ValueGeneratedNever();

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
            builder.Property(reservation => reservation.Id)
                .ValueGeneratedNever();

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
            builder.Property(transaction => transaction.Id)
                .ValueGeneratedNever();

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

        /// <summary>
        /// 配置 Inbox 消息表映射。
        /// </summary>
        /// <param name="builder">Inbox 消息实体配置器。</param>
        private static void ConfigureInboxMessage(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessages");

            builder.HasKey(inboxMessage => inboxMessage.Id);
            builder.Property(inboxMessage => inboxMessage.Id)
                .ValueGeneratedNever();

            builder.Property(inboxMessage => inboxMessage.EventId)
                .IsRequired();

            // 给 EventId 建唯一索引，从数据库层面保证同一集成事件不会被重复处理。
            builder.HasIndex(inboxMessage => inboxMessage.EventId)
                .IsUnique();

            builder.Property(inboxMessage => inboxMessage.EventType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(inboxMessage => inboxMessage.TenantId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(inboxMessage => inboxMessage.Payload)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(inboxMessage => inboxMessage.ProcessedAt)
                .IsRequired();
        }

        /// <summary>
        /// 配置 Outbox 消息表映射。
        /// </summary>
        /// <param name="builder">Outbox 消息实体配置器。</param>
        private static void ConfigureOutboxMessage(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(message => message.Id);

            builder.Property(message => message.EventType)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(message => message.Payload)
                .IsRequired();

            builder.Property(message => message.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(message => message.OccurredAt)
                .IsRequired();

            builder.Property(message => message.ProcessedAt);

            builder.Property(message => message.RetryCount)
                .IsRequired();

            // 后台发布器按状态筛选，并优先处理更早产生的消息。
            builder.HasIndex(message => new
            {
                message.Status,
                message.OccurredAt
            });
        }
    }
}
