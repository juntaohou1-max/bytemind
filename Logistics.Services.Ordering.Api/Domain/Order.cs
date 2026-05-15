namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// 发货订单聚合根。
    /// </summary>
    /// <remarks>
    /// 这里的订单不是商城订单，而是外部系统发送给仓储系统的一张发货指令。
    /// 第一阶段 Ordering 模块只负责接收、保存和查询订单，不处理库存、拣货和运输。
    /// </remarks>
    public class Order
    {
        private readonly List<OrderLine> _lines = [];
        private readonly List<OrderTimelineItem> _timelineItems = [];

        /// <summary>
        /// 订单在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 租户标识，用于区分不同客户或业务方的数据边界。
        /// </summary>
        public string TenantId { get; private set; } = string.Empty;

        /// <summary>
        /// 客户标识，表示这张发货指令属于哪个客户。
        /// </summary>
        public string CustomerId { get; private set; } = string.Empty;

        /// <summary>
        /// 外部系统传入的订单号。
        /// </summary>
        /// <remarks>
        /// 该字段用于和外部系统对账、查询和排查问题。
        /// 当前第一版要求它不能为空。
        /// </remarks>
        public string ExternalOrderNo { get; private set; } = string.Empty;

        /// <summary>
        /// 收货地址。
        /// </summary>
        public Address ReceiverAddress { get; private set; } = default!;

        /// <summary>
        /// 订单明细列表。
        /// </summary>
        /// <remarks>
        /// 对外只暴露只读集合，避免外部代码绕过订单聚合根直接修改明细。
        /// </remarks>
        public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

        /// <summary>
        /// 订单时间线记录。
        /// </summary>
        public IReadOnlyCollection<OrderTimelineItem> TimelineItems => _timelineItems.AsReadOnly();

        /// <summary>
        /// 订单当前状态。
        /// </summary>
        public OrderStatus Status { get; private set; }

        /// <summary>
        /// 订单创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// 创建一张发货订单。
        /// </summary>
        /// <param name="tenantId">租户标识。</param>
        /// <param name="customerId">客户标识。</param>
        /// <param name="externalOrderNo">外部系统传入的订单号。</param>
        /// <param name="receiverAddress">收货地址。</param>
        /// <param name="lines">订单明细集合，至少需要一条明细。</param>
        /// <exception cref="ArgumentException">必填文本为空，或明细集合为空时抛出。</exception>
        /// <exception cref="ArgumentNullException">收货地址或明细集合为 null 时抛出。</exception>
        public Order(
            string tenantId,
            string customerId,
            string externalOrderNo,
            Address receiverAddress,
            IEnumerable<OrderLine> lines)
        {
            Id = Guid.NewGuid();
            TenantId = EnsureRequired(tenantId, nameof(tenantId));
            CustomerId = EnsureRequired(customerId, nameof(customerId));
            ExternalOrderNo = EnsureRequired(externalOrderNo, nameof(externalOrderNo));
            ReceiverAddress = receiverAddress ?? throw new ArgumentNullException(nameof(receiverAddress), "订单必须有收货地址。");
            CreatedAt = DateTimeOffset.UtcNow;
            Status = OrderStatus.Created;

            AddInitialLines(lines);
            AddTimelineItem("OrderCreated", "订单已创建");
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private Order()
        {
        }

        /// <summary>
        /// 标记订单库存已锁定。
        /// </summary>
        /// <exception cref="InvalidOperationException">当前状态不允许标记库存已锁定时抛出。</exception>
        public void MarkInventoryReserved()
        {
            if (Status != OrderStatus.Created)
            {
                throw new InvalidOperationException("只有已创建的订单才能标记为库存已锁定。");
            }

            Status = OrderStatus.InventoryReserved;
            AddTimelineItem("InventoryReserved", "库存已锁定");
        }

        /// <summary>
        /// 标记订单履约单已创建。
        /// </summary>
        /// <exception cref="InvalidOperationException">当前状态不允许标记履约单已创建时抛出。</exception>
        public void MarkFulfillmentCreated()
        {
            if (Status != OrderStatus.InventoryReserved)
            {
                throw new InvalidOperationException("只有库存已锁定的订单才能标记为履约单已创建。");
            }

            Status = OrderStatus.FulfillmentCreated;
            AddTimelineItem("FulfillmentCreated", "履约单已创建");
        }

        /// <summary>
        /// 取消订单。
        /// </summary>
        /// <remarks>
        /// 当前第一版允许已创建、库存已锁定的订单取消。
        /// 履约单已创建后不能直接取消，后续需要走履约拦截或退货流程。
        /// </remarks>
        /// <exception cref="InvalidOperationException">当前状态不允许直接取消时抛出。</exception>
        public void Cancel()
        {
            if (Status == OrderStatus.Cancelled)
            {
                return;
            }

            if (Status == OrderStatus.FulfillmentCreated)
            {
                throw new InvalidOperationException("履约单已创建的订单不能直接取消。");
            }

            Status = OrderStatus.Cancelled;
            AddTimelineItem("OrderCancelled", "订单已取消");
        }

        private void AddInitialLines(IEnumerable<OrderLine> lines)
        {
            if (lines is null)
            {
                throw new ArgumentNullException(nameof(lines), "订单明细不能为空。");
            }

            var orderLines = lines.ToList();

            if (orderLines.Count == 0)
            {
                throw new ArgumentException("订单至少需要一条明细。", nameof(lines));
            }

            if (orderLines.Any(line => line is null))
            {
                throw new ArgumentException("订单明细不能包含空项。", nameof(lines));
            }

            _lines.AddRange(orderLines);
        }

        private void AddTimelineItem(string eventType, string description)
        {
            _timelineItems.Add(new OrderTimelineItem(
                eventType,
                description,
                DateTimeOffset.UtcNow));
        }

        private static string EnsureRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("订单必填字段不能为空。", parameterName);
            }

            return value.Trim();
        }
    }
}
