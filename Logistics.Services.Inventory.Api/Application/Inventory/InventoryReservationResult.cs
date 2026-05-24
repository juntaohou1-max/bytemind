namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// 库存预留结果，用于应用服务向接口层返回锁定库存后的预留信息。
    /// </summary>
    /// <param name="Id">库存预留 ID。</param>
    /// <param name="ExternalOrderNo">外部系统传入的订单号。</param>
    /// <param name="SkuId">SKU 标识。</param>
    /// <param name="Quantity">锁定库存数量。</param>
    /// <param name="Status">库存预留状态。</param>
    /// <param name="CreatedAt">库存预留创建时间。</param>
    public record InventoryReservationResult(
        Guid Id,
        string ExternalOrderNo,
        string SkuId,
        int Quantity,
        string Status,
        DateTimeOffset CreatedAt);
}
