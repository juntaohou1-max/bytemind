using Logistics.Services.Ordering.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Services.Ordering.Api.Infrastructure.Persistence
{
    public class OrderingDbContext : DbContext//告诉 EF Core，这是数据库上下文。
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options)//以后连接 SQL Server、配置连接字符串都会通过它传进来。
            : base(options)
        {

        }
        public DbSet<Order> Orders => Set<Order>();//告诉 EF Core，Order 这个聚合根将来会对应数据库里的订单表。

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();//保存待发布的集成事件，后续由后台任务负责发送。

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(builder => 
            {
                // Order 对应数据库 Orders 表
                builder.ToTable("Orders");

                // Order.Id 是 Orders 表的主键
                builder.HasKey(order => order.Id);

                builder.HasIndex(order => new
                {
                    order.TenantId,
                    order.ExternalOrderNo,

                }).IsUnique();

                builder.Property(order => order.TenantId)
                    .IsRequired()
                    .HasMaxLength(64);

                builder.Property(order => order.CustomerId)
                    .IsRequired()
                    .HasMaxLength(64);

                builder.Property(order => order.ExternalOrderNo)
                    .IsRequired()
                    .HasMaxLength(128);

                builder.Property(order => order.Status)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(64);

                builder.Property(order => order.CreatedAt)
                    .IsRequired();

                builder.OwnsOne(order => order.ReceiverAddress, address =>
                {
                    address.Property(a => a.ReceiverName)
                        .IsRequired()
                        .HasMaxLength(64);

                    address.Property(a => a.Phone)
                        .IsRequired()
                        .HasMaxLength(32);

                    address.Property(a => a.Province)
                        .IsRequired()
                        .HasMaxLength(64);

                    address.Property(a => a.City)
                        .IsRequired()
                        .HasMaxLength(64);

                    address.Property(a => a.District)
                        .IsRequired()
                        .HasMaxLength(64);

                    address.Property(a => a.Detail)
                        .IsRequired()
                        .HasMaxLength(256);
                });

                builder.OwnsMany(order => order.Lines, line =>
                {
                    line.ToTable("OrderLines");

                    line.WithOwner()
                        .HasForeignKey("OrderId");

                    line.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    line.HasKey("Id");

                    line.Property(l => l.SkuId)
                        .IsRequired()
                        .HasMaxLength(64);

                    line.Property(l => l.Quantity)
                        .IsRequired();
                });

                builder.Navigation(order => order.Lines)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                builder.OwnsMany(order => order.TimelineItems, timeline =>
                {
                    timeline.ToTable("OrderTimelineItems");

                    timeline.WithOwner()
                        .HasForeignKey("OrderId");

                    timeline.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    timeline.HasKey("Id");

                    timeline.Property(item => item.EventType)
                        .IsRequired()
                        .HasMaxLength(64);

                    timeline.Property(item => item.Description)
                        .IsRequired()
                        .HasMaxLength(256);

                    timeline.Property(item => item.OccurredAt)
                        .IsRequired();
                });

                builder.Navigation(order => order.TimelineItems)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<OutboxMessage>(builder =>
            {
                // OutboxMessage 对应数据库 OutboxMessages 表。
                builder.ToTable("OutboxMessages");

                // OutboxMessage.Id 是 OutboxMessages 表的主键。
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

                // 后台发布器通常会按状态筛选，并优先处理更早产生的消息。
                builder.HasIndex(message => new
                {
                    message.Status,
                    message.OccurredAt
                });
            });
        }
    }
}
