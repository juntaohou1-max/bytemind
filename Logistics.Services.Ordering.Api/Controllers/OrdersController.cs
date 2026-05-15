using Logistics.Services.Ordering.Api.Application.Orders;
using Logistics.Services.Ordering.Api.Contracts.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Services.Ordering.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderApplicationService _orderApplicationService;

        public OrdersController(IOrderApplicationService orderApplicationService)
        {
            _orderApplicationService = orderApplicationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            if (request.ReceiverAddress is null)
            {
                return BadRequest("收货地址不能为空。");
            }

            if (request.Lines is null || request.Lines.Count == 0)
            {
                return BadRequest("订单至少需要一条明细。");
            }

            try
            {
                var response = await _orderApplicationService.CreateAsync(request);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? status, DateTimeOffset? from, DateTimeOffset? to, string? externalOrderNo)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return BadRequest("开始时间不能晚于结束时间。");
            }

            try
            {
                var response = await _orderApplicationService.GetAllAsync(status, from, to, externalOrderNo);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _orderApplicationService.GetByIdAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var cancelled = await _orderApplicationService.CancelAsync(id);

                if (!cancelled)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id:guid}/timeline")]
        public async Task<IActionResult> GetTimeline(Guid id)
        {
            var response = await _orderApplicationService.GetTimelineAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPost("{id:guid}/mark-inventory-reserved")]
        public async Task<IActionResult> MarkInventoryReserved(Guid id)
        {
            try
            {
                var marked = await _orderApplicationService.MarkInventoryReservedAsync(id);

                if (!marked)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("{id:guid}/mark-fulfillment-created")]
        public async Task<IActionResult> MarkFulfillmentCreated(Guid id)
        {
            try
            {
                var marked = await _orderApplicationService.MarkFulfillmentCreatedAsync(id);

                if (!marked)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
