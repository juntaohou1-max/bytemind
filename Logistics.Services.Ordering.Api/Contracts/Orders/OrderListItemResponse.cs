namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class OrderListItemResponse
    {
        /// <summary>
        /// 系统生成的订单 ID。
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 租户标识。
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// 客户标识。
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// 外部系统传入的订单号。
        /// </summary>
        public string ExternalOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 订单当前状态。
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 订单创建时间。
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 订单明细行数。
        /// </summary>
        public int LineCount { get; set; }
    }
}
