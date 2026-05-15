using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Tests.Domain
{
    public class AddressTests
    {
        [Fact]
        public void Create_ShouldSucceed_WhenAllFieldsAreProvided()
        {
            var address = new Address(
                "张三",
                "13800000000",
                "浙江省",
                "杭州市",
                "西湖区",
                "文三路 100 号");

            Assert.Equal("张三", address.ReceiverName);
            Assert.Equal("13800000000", address.Phone);
            Assert.Equal("浙江省", address.Province);
            Assert.Equal("杭州市", address.City);
            Assert.Equal("西湖区", address.District);
            Assert.Equal("文三路 100 号", address.Detail);
        }

        [Fact]
        public void Create_ShouldTrimFields_WhenFieldsContainWhiteSpace()
        {
            var address = new Address(
                " 张三 ",
                " 13800000000 ",
                " 浙江省 ",
                " 杭州市 ",
                " 西湖区 ",
                " 文三路 100 号 ");

            Assert.Equal("张三", address.ReceiverName);
            Assert.Equal("13800000000", address.Phone);
            Assert.Equal("浙江省", address.Province);
            Assert.Equal("杭州市", address.City);
            Assert.Equal("西湖区", address.District);
            Assert.Equal("文三路 100 号", address.Detail);
        }

        [Theory]
        [InlineData("receiverName")]
        [InlineData("phone")]
        [InlineData("province")]
        [InlineData("city")]
        [InlineData("district")]
        [InlineData("detail")]
        public void Create_ShouldThrowException_WhenRequiredFieldIsEmpty(string emptyField)
        {
            var receiverName = "张三";
            var phone = "13800000000";
            var province = "浙江省";
            var city = "杭州市";
            var district = "西湖区";
            var detail = "文三路 100 号";

            switch (emptyField)
            {
                case "receiverName":
                    receiverName = "";
                    break;
                case "phone":
                    phone = "";
                    break;
                case "province":
                    province = "";
                    break;
                case "city":
                    city = "";
                    break;
                case "district":
                    district = "";
                    break;
                case "detail":
                    detail = "";
                    break;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new Address(
                    receiverName,
                    phone,
                    province,
                    city,
                    district,
                    detail));

            Assert.Equal(emptyField, exception.ParamName);
        }
    }
}
