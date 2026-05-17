using System;

namespace Logistics.Services.Inventory.Api.Domain
{
    /// <summary>
    /// SKU 库存总账，表示某个 SKU 当前的库存状态。
    /// </summary>
    public class InventoryItem
    {
        private readonly List<InventoryReservation> _reservations = [];
        private readonly List<InventoryTransaction> _transactions = [];

        /// <summary>
        /// 库存总账在当前系统内的唯一标识。
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// 当前库存总账对应的 SKU 标识。
        /// </summary>
        public string SkuId { get; private set; } = string.Empty;

        /// <summary>
        /// 当前仓库内实际存放的库存数量。
        /// </summary>
        public int OnHandQuantity { get; private set; }

        /// <summary>
        /// 已经被订单锁定但尚未实际出库的库存数量。
        /// </summary>
        public int ReservedQuantity { get; private set; }

        /// <summary>
        /// 当前仓库内已损坏且不可正常使用的库存数量。
        /// </summary>
        public int DamagedQuantity { get; private set; }

        /// <summary>
        /// 当前可继续分配给新订单的库存数量。
        /// </summary>
        public int AvailableQuantity => OnHandQuantity - ReservedQuantity - DamagedQuantity;

        /// <summary>
        /// 当前 SKU 的库存预留记录。
        /// </summary>
        public IReadOnlyCollection<InventoryReservation> Reservations => _reservations.AsReadOnly();

        /// <summary>
        /// 当前 SKU 的库存流水记录。
        /// </summary>
        public IReadOnlyCollection<InventoryTransaction> Transactions => _transactions.AsReadOnly();

        /// <summary>
        /// 创建一个 SKU 库存总账。
        /// </summary>
        /// <param name="skuId">当前库存总账对应的 SKU 标识。</param>
        public InventoryItem(string skuId)
        {

            if (string.IsNullOrWhiteSpace(skuId))
                throw new ArgumentException("SKU 标识不能为空。", nameof(skuId));

            SkuId = skuId.Trim();
            Id = Guid.NewGuid();
            OnHandQuantity = 0;
            ReservedQuantity = 0;
            DamagedQuantity = 0;
        }

        /// <summary>
        /// 保留给 EF Core 等 ORM 使用的无参构造函数。
        /// </summary>
        private InventoryItem()
        {
        }

        /// <summary>
        /// 调整在手库存并生成库存流水。
        /// </summary>
        /// <param name="quantityDelta">库存增减变化量，正数表示增加，负数表示减少。</param>
        /// <param name="referenceNo">外部参考号，例如盘点单号或入库单号。</param>
        public void Adjust(int quantityDelta, string? referenceNo = null)
        {
            if (quantityDelta == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantityDelta), "库存调整数量不能等于 0。");
            }

            var adjustedOnHandQuantity = OnHandQuantity + quantityDelta;

            if (adjustedOnHandQuantity < 0)
            {
                throw new InvalidOperationException("调整后在手库存不能小于 0。");
            }

            OnHandQuantity = adjustedOnHandQuantity;

            _transactions.Add(new InventoryTransaction(
                SkuId,
                InventoryTransactionType.Adjustment,
                quantityDelta,
                referenceNo: referenceNo));
        }

        /// <summary>
        /// 为外部订单锁定库存并创建库存预留。
        /// </summary>
        /// <param name="externalOrderNo">外部系统传入的订单号。</param>
        /// <param name="quantity">本次需要锁定的库存数量。</param>
        public InventoryReservation Reserve(string externalOrderNo, int quantity)
        {
            //库存足够才能锁定
            //锁定成功后 ReservedQuantity 增加
            //创建 InventoryReservation
            //加入 Reservations 集合
            //生成 ReservationCreated 库存流水
            //返回 reservation
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "锁定数量必须大于 0。");
            }

            if (quantity > AvailableQuantity)
            {
                throw new InvalidOperationException("可用库存不足，无法锁定库存。");
            }

            var reservation = new InventoryReservation(externalOrderNo, SkuId, quantity);

            ReservedQuantity += quantity;
            _reservations.Add(reservation);
            _transactions.Add(new InventoryTransaction(
                SkuId,
                InventoryTransactionType.ReservationCreated,
                quantity,
                reservation.Id,
                externalOrderNo));

            return reservation;
        }

        /// <summary>
        /// 释放一条库存预留并生成库存流水。
        /// </summary>
        /// <param name="reservationId">要释放的库存预留标识。</param>
        public void ReleaseReservation(Guid reservationId)
        {
            //根据 reservationId 找到预留记录
            //只有 Active 的 reservation 能释放
            //释放后 reservation.Status = Released
            //ReservedQuantity -= reservation.Quantity
            //生成 ReservationReleased 流水
            //OnHandQuantity 不变
            //AvailableQuantity 增加
            var reservation = _reservations.FirstOrDefault(s => s.Id == reservationId);

            if (reservation is null)
            {
                throw new ArgumentOutOfRangeException(nameof(reservationId), "未找到库存预留。");
            }

            reservation.Release();

            ReservedQuantity -= reservation.Quantity;

            var inventoryTransaction = new InventoryTransaction(
                SkuId,
                InventoryTransactionType.ReservationReleased,
                -reservation.Quantity,
                reservation.Id,
                reservation.ExternalOrderNo);

            _transactions.Add(inventoryTransaction);
        }

        /// <summary>
        /// 扣减一条库存预留并生成库存流水。
        /// </summary>
        /// <param name="reservationId">要扣减的库存预留标识。</param>
        public void DeductReservation(Guid reservationId)
        {
            var reservation = _reservations.FirstOrDefault(s => s.Id == reservationId);

            if (reservation is null)
            {
                throw new ArgumentOutOfRangeException(nameof(reservationId), "未找到库存预留。");
            }

            reservation.Deduct();

            OnHandQuantity -= reservation.Quantity;
            ReservedQuantity -= reservation.Quantity;

            _transactions.Add(new InventoryTransaction(
                SkuId,
                InventoryTransactionType.ReservationDeducted,
                -reservation.Quantity,
                reservation.Id,
                reservation.ExternalOrderNo));
        }
    }
}
