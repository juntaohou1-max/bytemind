using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Tests.Domain
{
    public class OutboxMessageTests
    {
        [Fact]
        public void Create_ShouldSetPendingStatus_WhenRequiredFieldsAreProvided()
        {
            var occurredAt = DateTimeOffset.UtcNow;

            var message = new OutboxMessage(
                " OrderCreated ",
                " {\"orderId\":\"order-001\"} ",
                occurredAt);

            Assert.NotEqual(Guid.Empty, message.Id);
            Assert.Equal("OrderCreated", message.EventType);
            Assert.Equal("{\"orderId\":\"order-001\"}", message.Payload);
            Assert.Equal(OutboxStatus.Pending, message.Status);
            Assert.Equal(occurredAt, message.OccurredAt);
            Assert.Null(message.ProcessedAt);
            Assert.Equal(0, message.RetryCount);
        }

        [Theory]
        [InlineData("eventType")]
        [InlineData("payload")]
        public void Create_ShouldThrowException_WhenRequiredTextFieldIsEmpty(string emptyField)
        {
            var eventType = "OrderCreated";
            var payload = "{\"orderId\":\"order-001\"}";

            switch (emptyField)
            {
                case "eventType":
                    eventType = "";
                    break;
                case "payload":
                    payload = "";
                    break;
            }

            var exception = Assert.Throws<ArgumentException>(() =>
                new OutboxMessage(eventType, payload));

            Assert.Equal(emptyField, exception.ParamName);
        }

        [Fact]
        public void MarkPublishing_ShouldChangeStatusToPublishing()
        {
            var message = CreateMessage();

            message.MarkPublishing();

            Assert.Equal(OutboxStatus.Publishing, message.Status);
        }

        [Fact]
        public void MarkPublished_ShouldSetStatusAndProcessedAt()
        {
            var message = CreateMessage();
            var processedAt = DateTimeOffset.UtcNow;

            message.MarkPublished(processedAt);

            Assert.Equal(OutboxStatus.Published, message.Status);
            Assert.Equal(processedAt, message.ProcessedAt);
        }

        [Fact]
        public void MarkFailed_ShouldSetFailedStatusAndIncreaseRetryCount()
        {
            var message = CreateMessage();

            message.MarkFailed();
            message.MarkFailed();

            Assert.Equal(OutboxStatus.Failed, message.Status);
            Assert.Equal(2, message.RetryCount);
            Assert.Null(message.ProcessedAt);
        }

        [Fact]
        public void MarkFailed_ShouldThrowException_WhenMessageIsPublished()
        {
            var message = CreateMessage();

            message.MarkPublished(DateTimeOffset.UtcNow);

            Assert.Throws<InvalidOperationException>(() => message.MarkFailed());
        }

        [Fact]
        public void CanRetry_ShouldReturnTrue_WhenFailedRetryCountIsLessThanMaxRetryCount()
        {
            var message = CreateMessage();

            message.MarkFailed();

            Assert.True(message.CanRetry(3));
        }

        [Fact]
        public void CanRetry_ShouldReturnFalse_WhenRetryLimitIsReached()
        {
            var message = CreateMessage();

            message.MarkFailed();
            message.MarkFailed();
            message.MarkFailed();

            Assert.False(message.CanRetry(3));
        }

        [Fact]
        public void MarkPendingForRetry_ShouldChangeFailedMessageToPending()
        {
            var message = CreateMessage();

            message.MarkFailed();
            message.MarkPendingForRetry();

            Assert.Equal(OutboxStatus.Pending, message.Status);
            Assert.Equal(1, message.RetryCount);
            Assert.Null(message.ProcessedAt);
        }

        [Fact]
        public void MarkPendingForRetry_ShouldThrowException_WhenMessageIsNotFailed()
        {
            var message = CreateMessage();

            Assert.Throws<InvalidOperationException>(() => message.MarkPendingForRetry());
        }

        private static OutboxMessage CreateMessage()
        {
            return new OutboxMessage(
                "OrderCreated",
                "{\"orderId\":\"order-001\"}");
        }
    }
}
