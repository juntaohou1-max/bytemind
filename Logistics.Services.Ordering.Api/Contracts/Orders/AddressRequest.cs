namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public class AddressRequest
    {
        /// <summary>
        /// 收货人姓名。
        /// </summary>
        public string? ReceiverName { get; set; }

        /// <summary>
        /// 收货人联系电话。
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 省份。
        /// </summary>
        public string? Province { get; set; }

        /// <summary>
        /// 城市。
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 区县。
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 详细地址，例如街道、门牌号、园区、楼层等。
        /// </summary>
        public string? Detail { get; set; }
    }
}
