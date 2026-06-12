using Domain = Logistics.Services.Warehouse.Api.Domain;
using Logistics.Services.Warehouse.Api.Domain.Inbox;
using Logistics.Services.Warehouse.Api.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Services.Warehouse.Api.Infrastructure.Persistence
{
    /// <summary>
    /// Warehouse 模块数据库上下文，管理仓库领域模型的持久化。
    /// </summary>
    public class WarehouseDbContext : DbContext
    {
        /// <summary>
        /// 创建 Warehouse 数据库上下文。
        /// </summary>
        /// <param name="options">数据库上下文配置。</param>
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 仓库集合。
        /// </summary>
        public DbSet<Domain.Warehouse> Warehouses => Set<Domain.Warehouse>();

        /// <summary>
        /// 区域集合。
        /// </summary>
        public DbSet<Domain.Zone> Zones => Set<Domain.Zone>();

        /// <summary>
        /// 货位集合。
        /// </summary>
        public DbSet<Domain.BinLocation> BinLocations => Set<Domain.BinLocation>();

        /// <summary>
        /// Inbox 消息集合，用于记录已处理的集成事件，实现幂等消费。
        /// </summary>
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        /// <summary>
        /// Outbox 消息集合，用于保存待发布的集成事件，后续由后台任务负责发送。
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        /// <summary>
        /// 配置 Warehouse 模块领域模型到数据库表的映射。
        /// </summary>
        /// <param name="modelBuilder">模型构建器。</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureWarehouse(modelBuilder.Entity<Domain.Warehouse>());
            ConfigureZone(modelBuilder.Entity<Domain.Zone>());
            ConfigureBinLocation(modelBuilder.Entity<Domain.BinLocation>());
            ConfigureInboxMessage(modelBuilder.Entity<InboxMessage>());
            ConfigureOutboxMessage(modelBuilder.Entity<OutboxMessage>());
        }

        /// <summary>
        /// 配置仓库表映射。
        /// </summary>
        /// <param name="builder">仓库实体配置器。</param>
        private static void ConfigureWarehouse(EntityTypeBuilder<Domain.Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(w => w.Id);
            builder.Property(w => w.Id)
                .ValueGeneratedNever();

            builder.Property(w => w.Code)
                .HasMaxLength(100)
                .IsRequired();

            // 仓库编码建索引，方便按编码快速查找。
            builder.HasIndex(w => w.Code);

            builder.Property(w => w.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(w => w.Address)
                .HasMaxLength(500)
                .IsRequired();

            // 仓库状态存为字符串，便于直接在数据库里看懂状态值。
            builder.Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(w => w.TenantId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(w => w.CreatedAt)
                .IsRequired();

            builder.Property(w => w.UpdatedAt)
                .IsRequired();

            // 一对多：Warehouse → Zone
            builder.HasMany(w => w.Zones)
                .WithOne()
                .HasForeignKey("WarehouseId")
                .OnDelete(DeleteBehavior.Cascade);

            // 通过私有字段 _zones 加载导航属性，避免 EF Core 尝试 set Zones 属性。
            builder.Navigation(w => w.Zones)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }

        /// <summary>
        /// 配置区域表映射。
        /// </summary>
        /// <param name="builder">区域实体配置器。</param>
        private static void ConfigureZone(EntityTypeBuilder<Domain.Zone> builder)
        {
            builder.ToTable("Zones");

            builder.HasKey(z => z.Id);
            builder.Property(z => z.Id)
                .ValueGeneratedNever();

            builder.Property(z => z.WarehouseId)
                .IsRequired();

            builder.Property(z => z.Name)
                .HasMaxLength(200)
                .IsRequired();

            // 区域类型存为字符串。
            builder.Property(z => z.ZoneType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            // 给外键建索引，方便按仓库查询区域。
            builder.HasIndex(z => z.WarehouseId);

            // 一对多：Zone → BinLocation
            builder.HasMany(z => z.Bins)
                .WithOne()
                .HasForeignKey("ZoneId")
                .OnDelete(DeleteBehavior.Cascade);

            // 通过私有字段 _bins 加载导航属性。
            builder.Navigation(z => z.Bins)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }

        /// <summary>
        /// 配置货位表映射。
        /// </summary>
        /// <param name="builder">货位实体配置器。</param>
        private static void ConfigureBinLocation(EntityTypeBuilder<Domain.BinLocation> builder)
        {
            builder.ToTable("BinLocations");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .ValueGeneratedNever();

            builder.Property(b => b.ZoneId)
                .IsRequired();

            builder.Property(b => b.Code)
                .HasMaxLength(50)
                .IsRequired();

            // 货位状态存为字符串。
            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            // SkuId 可空：空货位不存放任何 SKU。
            builder.Property(b => b.SkuId)
                .HasMaxLength(100)
                .IsRequired(false);

            // 给外键建索引，方便按区域查询货位。
            builder.HasIndex(b => b.ZoneId);
        }

        /// <summary>
        /// 配置 Inbox 消息表映射。
        /// </summary>
        /// <param name="builder">Inbox 消息实体配置器。</param>
        private static void ConfigureInboxMessage(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessages");

            builder.HasKey(message => message.Id);
            builder.Property(message => message.Id)
                .ValueGeneratedNever();

            builder.Property(message => message.EventId)
                .IsRequired();

            // 给 EventId 建唯一索引，从数据库层面保证同一集成事件不会被重复处理。
            builder.HasIndex(message => message.EventId)
                .IsUnique();

            builder.Property(message => message.EventType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(message => message.TenantId)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(message => message.Payload)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(message => message.ProcessedAt)
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
