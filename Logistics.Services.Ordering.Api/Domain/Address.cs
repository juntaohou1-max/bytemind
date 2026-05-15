namespace Logistics.Services.Ordering.Api.Domain
{
    /// <summary>
    /// 收货地址值对象。
    /// </summary>
    /// <remarks>
    /// 地址本身没有独立身份，不能脱离订单单独存在。
    /// 在当前阶段，它作为 <see cref="Order"/> 的一部分，用来描述这张发货指令要送达的位置。
    /// </remarks>
    public class Address
    {
        /// <summary>
        /// 收货人姓名。
        /// </summary>
        public string ReceiverName { get; private set; } = string.Empty;

        /// <summary>
        /// 收货人联系电话。
        /// </summary>
        public string Phone { get; private set; } = string.Empty;

        /// <summary>
        /// 省份。
        /// </summary>
        public string Province { get; private set; } = string.Empty;

        /// <summary>
        /// 城市。
        /// </summary>
        public string City { get; private set; } = string.Empty;

        /// <summary>
        /// 区县。
        /// </summary>
        public string District { get; private set; } = string.Empty;

        /// <summary>
        /// 详细地址，例如街道、门牌号、园区、楼层等。
        /// </summary>
        public string Detail { get; private set; } = string.Empty;

        /// <summary>
        /// 创建收货地址。
        /// </summary>
        /// <param name="receiverName">收货人姓名。</param>
        /// <param name="phone">收货人联系电话。</param>
        /// <param name="province">省份。</param>
        /// <param name="city">城市。</param>
        /// <param name="district">区县。</param>
        /// <param name="detail">详细地址。</param>
        /// <exception cref="ArgumentException">任一必填字段为空时抛出。</exception>
        public Address(
            string receiverName,
            string phone,
            string province,
            string city,
            string district,
            string detail)
        {
            ReceiverName = EnsureRequired(receiverName, nameof(receiverName));
            Phone = EnsureRequired(phone, nameof(phone));
            Province = EnsureRequired(province, nameof(province));
            City = EnsureRequired(city, nameof(city));
            District = EnsureRequired(district, nameof(district));
            Detail = EnsureRequired(detail, nameof(detail));
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private Address()
        {
        }

        private static string EnsureRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("地址信息中的必填字段不能为空。", parameterName);
            }

            return value.Trim();
        }
    }
}
