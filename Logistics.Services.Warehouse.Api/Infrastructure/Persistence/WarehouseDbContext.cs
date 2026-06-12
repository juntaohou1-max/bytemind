using Domain = Logistics.Services.Warehouse.Api.Domain;
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
        /// 配置 Warehouse 模块领域模型到数据库表的映射。
        /// </summary>
        /// <param name="modelBuilder">模型构建器。</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureWarehouse(modelBuilder.Entity<Domain.Warehouse>());
            ConfigureZone(modelBuilder.Entity<Domain.Zone>());
            ConfigureBinLocation(modelBuilder.Entity<Domain.BinLocation>());
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
    }
}
