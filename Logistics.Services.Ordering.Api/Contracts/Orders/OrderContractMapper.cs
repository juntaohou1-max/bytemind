using Logistics.Services.Ordering.Api.Domain;

namespace Logistics.Services.Ordering.Api.Contracts.Orders
{
    public static class OrderContractMapper
    {
        public static Order ToOrder(CreateOrderRequest request) 
        {

            var address = new Address(
                request.ReceiverAddress!.ReceiverName!,
                request.ReceiverAddress.Phone!,
                request.ReceiverAddress.Province!,
                request.ReceiverAddress.City!,
                request.ReceiverAddress.District!,
                request.ReceiverAddress.Detail!);

            var lines = request.Lines!
                .Select(line => new OrderLine(line.SkuId!, line.Quantity))
                .ToList();

            return new Order(
                request.TenantId!,
                request.CustomerId!,
                request.ExternalOrderNo!,
                address,
                lines);
        }

        public static OrderDetailResponse ToDetailResponse(Order order)
        {
            return new OrderDetailResponse
            {
                Id = order.Id,
                TenantId = order.TenantId,
                CustomerId = order.CustomerId,
                ExternalOrderNo = order.ExternalOrderNo,
                ReceiverAddress = new AddressResponse
                {
                    ReceiverName = order.ReceiverAddress.ReceiverName,
                    Phone = order.ReceiverAddress.Phone,
                    Province = order.ReceiverAddress.Province,
                    City = order.ReceiverAddress.City,
                    District = order.ReceiverAddress.District,
                    Detail = order.ReceiverAddress.Detail
                },
                Lines = order.Lines
                    .Select(line => new OrderLineResponse
                    {
                        SkuId = line.SkuId,
                        Quantity = line.Quantity
                    })
                    .ToList(),
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            };
        }

        public static OrderListItemResponse ToListItemResponse(Order order)
        {
            return new OrderListItemResponse
            {
                Id = order.Id,
                TenantId = order.TenantId,
                CustomerId = order.CustomerId,
                ExternalOrderNo = order.ExternalOrderNo,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                LineCount = order.Lines.Count
            };
        }

        public static IReadOnlyCollection<OrderTimelineItemResponse> ToTimelineResponse(Order order)
        {
            return order.TimelineItems
                .Select(item => new OrderTimelineItemResponse
                {
                    EventType = item.EventType,
                    Description = item.Description,
                    OccurredAt = item.OccurredAt
                })
                .ToList();
        }

        /// <summary>
        /// 将当前页数据和分页元数据组装成统一分页响应。
        /// </summary>
        /// <typeparam name="T">当前页中单条数据的响应类型。</typeparam>
        /// <param name="items">当前页数据。</param>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数据条数。</param>
        /// <param name="totalCount">符合查询条件的总条数。</param>
        /// <returns>分页响应。</returns>
        public static Logistics.Services.Ordering.Api.Contracts.PagedResponse<T> ToPagedResponse<T>(
            IReadOnlyCollection<T> items,
            int pageNumber,
            int pageSize,
            int totalCount)
        {
            return new Logistics.Services.Ordering.Api.Contracts.PagedResponse<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
