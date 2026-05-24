namespace Logistics.Services.Inventory.Api.Application.Inventory
{
    /// <summary>
    /// 锁定库存命令，表示外部订单占用某个 SKU 库存的输入数据。
    /// </summary>
    /// <param name="SkuId">要锁定库存的 SKU 标识。</param>
    /// <param name="ExternalOrderNo">外部系统传入的订单号。</param>
    /// <param name="Quantity">本次需要锁定的库存数量。</param>
    public record ReserveInventoryCommand(
        string SkuId,
        string ExternalOrderNo,
        int Quantity);
}
