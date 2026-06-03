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
            var item = await _inventoryApplicationService.GetBySkuIdAsync(skuId);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(InventoryItemResponse.FromResult(item));
        }

        /// <summary>
        /// 调整库存。
        /// </summary>
        /// <param name="request">调整库存请求。</param>
        [HttpPost("adjustments")]
        public async Task<ActionResult<InventoryItemResponse>> AdjustInventory([FromBody] AdjustInventoryRequest request)
        {
            var command = new AdjustInventoryCommand(
                request.SkuId,
                request.QuantityDelta,
                request.ReferenceNo);

            var item = await _inventoryApplicationService.AdjustInventoryAsync(command);

            return Ok(InventoryItemResponse.FromResult(item));
        }
    }
}
