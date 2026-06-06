using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Contracts.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Services.Inventory.Api.Controllers
{
    /// <summary>
    /// 库存接口控制器。
    /// </summary>
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryApplicationService _inventoryApplicationService;

        /// <summary>
        /// 创建库存接口控制器。
        /// </summary>
        /// <param name="iInventoryApplicationService">库存应用服务。</param>
        public InventoryController(IInventoryApplicationService iInventoryApplicationService)
        {
            _inventoryApplicationService = iInventoryApplicationService;
        }

        /// <summary>
        /// 根据 SKU 查询库存总账。
        /// </summary>
        /// <param name="skuId">SKU 标识。</param>
        [HttpGet("skus/{skuId}")]
        public async Task<ActionResult<InventoryItemResponse>> GetBySkuId(string skuId)
        {
            try
            {
                var item = await _inventoryApplicationService.GetBySkuIdAsync(skuId);

                if (item is null)
                {
                    return NotFound(CreateProblemDetails("库存总账不存在", $"SKU '{skuId}' 的库存总账不存在。"));
                }

                return Ok(InventoryItemResponse.FromResult(item));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", exception.Message));
            }
        }

        /// <summary>
        /// 调整库存。
        /// </summary>
        /// <param name="request">调整库存请求。</param>
        [HttpPost("adjustments")]
        public async Task<ActionResult<InventoryItemResponse>> AdjustInventory([FromBody] AdjustInventoryRequest? request)
        {
            if (request is null)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", "请求体不能为空。"));
            }

            try
            {
                var command = new AdjustInventoryCommand(
                    request.SkuId,
                    request.QuantityDelta,
                    request.ReferenceNo);

                var item = await _inventoryApplicationService.AdjustInventoryAsync(command);

                return Ok(InventoryItemResponse.FromResult(item));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(CreateProblemDetails("库存调整失败", exception.Message));
            }
        }

        /// <summary>
        /// 锁定库存。
        /// </summary>
        /// <param name="request">锁定库存请求。</param>
        [HttpPost("reservations")]
        public async Task<ActionResult<InventoryReservationResponse>> ReserveInventory([FromBody] ReserveInventoryRequest? request)
        {
            if (request is null)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", "请求体不能为空。"));
            }

            try
            {
                var command = new ReserveInventoryCommand(
                    request.SkuId,
                    request.ExternalOrderNo,
                    request.Quantity);

                var reservation = await _inventoryApplicationService.ReserveInventoryAsync(command);

                return Ok(InventoryReservationResponse.FromResult(reservation));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(CreateProblemDetails("库存总账不存在", exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(CreateProblemDetails("库存锁定失败", exception.Message));
            }
        }

        /// <summary>
        /// 释放库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        [HttpPost("reservations/{reservationId}/release")]
        public async Task<ActionResult> ReleaseReservation([FromRoute] Guid reservationId)
        {
            if (reservationId == Guid.Empty)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", "库存预留 ID 不能为空。"));
            }

            try
            {
                await _inventoryApplicationService.ReleaseReservationAsync(reservationId);

                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(CreateProblemDetails("库存预留不存在", exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(CreateProblemDetails("库存预留释放失败", exception.Message));
            }
        }

        /// <summary>
        /// 扣减库存预留。
        /// </summary>
        /// <param name="reservationId">库存预留 ID。</param>
        [HttpPost("reservations/{reservationId}/deduct")]
        public async Task<ActionResult> DeductReservation([FromRoute] Guid reservationId)
        {
            if (reservationId == Guid.Empty)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", "库存预留 ID 不能为空。"));
            }

            try
            {
                await _inventoryApplicationService.DeductReservationAsync(reservationId);

                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(CreateProblemDetails("库存预留不存在", exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(CreateProblemDetails("请求参数无效", exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(CreateProblemDetails("库存预留扣减失败", exception.Message));
            }
        }

        /// <summary>
        /// 创建接口错误响应内容。
        /// </summary>
        /// <param name="title">错误标题。</param>
        /// <param name="detail">错误详情。</param>
        private static ProblemDetails CreateProblemDetails(string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail
            };
        }
    }
}
