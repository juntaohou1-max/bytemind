using Logistics.Services.Inventory.Api.Application.Inventory;
using Logistics.Services.Inventory.Api.Contracts.Inventory;
using Logistics.Services.Inventory.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Services.Inventory.Tests.Controllers
{
    /// <summary>
    /// 库存接口控制器测试。
    /// </summary>
    public class InventoryControllerTests
    {
        /// <summary>
        /// 根据 SKU 查询库存时，库存总账不存在应该返回 404。
        /// </summary>
        [Fact]
        public async Task GetBySkuId_ShouldReturnNotFound_WhenInventoryItemDoesNotExist()
        {
            var service = new FakeInventoryApplicationService
            {
                GetBySkuIdAsyncHandler = (_, _) => Task.FromResult<InventoryItemResult?>(null)
            };
            var controller = new InventoryController(service);

            var response = await controller.GetBySkuId("SKU-404");

            var problemDetails = AssertProblemDetails(response.Result, 404);
            Assert.Equal("库存总账不存在", problemDetails.Title);
        }

        /// <summary>
        /// 根据 SKU 查询库存时，SKU 为空应该返回 400。
        /// </summary>
        [Fact]
        public async Task GetBySkuId_ShouldReturnBadRequest_WhenSkuIdIsEmpty()
        {
            var service = new FakeInventoryApplicationService
            {
                GetBySkuIdAsyncHandler = (_, _) => throw new ArgumentException("SKU 标识不能为空。", "skuId")
            };
            var controller = new InventoryController(service);

            var response = await controller.GetBySkuId(" ");

            var problemDetails = AssertProblemDetails(response.Result, 400);
            Assert.Equal("请求参数无效", problemDetails.Title);
        }

        /// <summary>
        /// 调整库存时，请求体为空应该返回 400。
        /// </summary>
        [Fact]
        public async Task AdjustInventory_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            var service = new FakeInventoryApplicationService();
            var controller = new InventoryController(service);

            var response = await controller.AdjustInventory(null);

            var problemDetails = AssertProblemDetails(response.Result, 400);
            Assert.Equal("请求参数无效", problemDetails.Title);
        }

        /// <summary>
        /// 调整库存时，调整数量为 0 应该返回 400。
        /// </summary>
        [Fact]
        public async Task AdjustInventory_ShouldReturnBadRequest_WhenQuantityDeltaIsZero()
        {
            var service = new FakeInventoryApplicationService
            {
                AdjustInventoryAsyncHandler = (_, _) =>
                    throw new ArgumentOutOfRangeException("quantityDelta", "库存调整数量不能等于 0。")
            };
            var controller = new InventoryController(service);

            var response = await controller.AdjustInventory(new AdjustInventoryRequest
            {
                SkuId = "SKU-001",
                QuantityDelta = 0
            });

            var problemDetails = AssertProblemDetails(response.Result, 400);
            Assert.Equal("请求参数无效", problemDetails.Title);
        }

        /// <summary>
        /// 锁定库存时，SKU 不存在应该返回 404。
        /// </summary>
        [Fact]
        public async Task ReserveInventory_ShouldReturnNotFound_WhenInventoryItemDoesNotExist()
        {
            var service = new FakeInventoryApplicationService
            {
                ReserveInventoryAsyncHandler = (_, _) => throw new KeyNotFoundException("库存总账不存在。")
            };
            var controller = new InventoryController(service);

            var response = await controller.ReserveInventory(new ReserveInventoryRequest
            {
                SkuId = "SKU-404",
                ExternalOrderNo = "ERP-001",
                Quantity = 10
            });

            var problemDetails = AssertProblemDetails(response.Result, 404);
            Assert.Equal("库存总账不存在", problemDetails.Title);
        }

        /// <summary>
        /// 锁定库存时，可用库存不足应该返回 400。
        /// </summary>
        [Fact]
        public async Task ReserveInventory_ShouldReturnBadRequest_WhenAvailableQuantityIsNotEnough()
        {
            var service = new FakeInventoryApplicationService
            {
                ReserveInventoryAsyncHandler = (_, _) => throw new InvalidOperationException("可用库存不足，无法锁定库存。")
            };
            var controller = new InventoryController(service);

            var response = await controller.ReserveInventory(new ReserveInventoryRequest
            {
                SkuId = "SKU-001",
                ExternalOrderNo = "ERP-001",
                Quantity = 999
            });

            var problemDetails = AssertProblemDetails(response.Result, 400);
            Assert.Equal("库存锁定失败", problemDetails.Title);
        }

        /// <summary>
        /// 释放库存预留时，预留 ID 为空应该返回 400。
        /// </summary>
        [Fact]
        public async Task ReleaseReservation_ShouldReturnBadRequest_WhenReservationIdIsEmpty()
        {
            var service = new FakeInventoryApplicationService();
            var controller = new InventoryController(service);

            var response = await controller.ReleaseReservation(Guid.Empty);

            var problemDetails = AssertProblemDetails(response, 400);
            Assert.Equal("请求参数无效", problemDetails.Title);
            Assert.Equal(0, service.ReleaseReservationCallCount);
        }

        /// <summary>
        /// 释放库存预留时，预留不存在应该返回 404。
        /// </summary>
        [Fact]
        public async Task ReleaseReservation_ShouldReturnNotFound_WhenReservationDoesNotExist()
        {
            var service = new FakeInventoryApplicationService
            {
                ReleaseReservationAsyncHandler = (_, _) => throw new KeyNotFoundException("库存预留不存在。")
            };
            var controller = new InventoryController(service);

            var response = await controller.ReleaseReservation(Guid.NewGuid());

            var problemDetails = AssertProblemDetails(response, 404);
            Assert.Equal("库存预留不存在", problemDetails.Title);
        }

        /// <summary>
        /// 释放库存预留时，预留状态不允许释放应该返回 400。
        /// </summary>
        [Fact]
        public async Task ReleaseReservation_ShouldReturnBadRequest_WhenReservationStatusCannotRelease()
        {
            var service = new FakeInventoryApplicationService
            {
                ReleaseReservationAsyncHandler = (_, _) => throw new InvalidOperationException("只有Active订单才能标记为已释放。")
            };
            var controller = new InventoryController(service);

            var response = await controller.ReleaseReservation(Guid.NewGuid());

            var problemDetails = AssertProblemDetails(response, 400);
            Assert.Equal("库存预留释放失败", problemDetails.Title);
        }

        /// <summary>
        /// 扣减库存预留时，预留不存在应该返回 404。
        /// </summary>
        [Fact]
        public async Task DeductReservation_ShouldReturnNotFound_WhenReservationDoesNotExist()
        {
            var service = new FakeInventoryApplicationService
            {
                DeductReservationAsyncHandler = (_, _) => throw new KeyNotFoundException("库存预留不存在。")
            };
            var controller = new InventoryController(service);

            var response = await controller.DeductReservation(Guid.NewGuid());

            var problemDetails = AssertProblemDetails(response, 404);
            Assert.Equal("库存预留不存在", problemDetails.Title);
        }

        /// <summary>
        /// 扣减库存预留时，预留状态不允许扣减应该返回 400。
        /// </summary>
        [Fact]
        public async Task DeductReservation_ShouldReturnBadRequest_WhenReservationStatusCannotDeduct()
        {
            var service = new FakeInventoryApplicationService
            {
                DeductReservationAsyncHandler = (_, _) => throw new InvalidOperationException("只有Active订单才能标记为已扣减。")
            };
            var controller = new InventoryController(service);

            var response = await controller.DeductReservation(Guid.NewGuid());

            var problemDetails = AssertProblemDetails(response, 400);
            Assert.Equal("库存预留扣减失败", problemDetails.Title);
        }

        /// <summary>
        /// 断言接口返回 ProblemDetails 错误内容。
        /// </summary>
        /// <param name="actionResult">接口返回结果。</param>
        /// <param name="expectedStatusCode">预期 HTTP 状态码。</param>
        private static ProblemDetails AssertProblemDetails(IActionResult? actionResult, int expectedStatusCode)
        {
            var objectResult = Assert.IsAssignableFrom<ObjectResult>(actionResult);
            Assert.Equal(expectedStatusCode, objectResult.StatusCode);
            return Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        /// <summary>
        /// 用于控制器测试的库存应用服务假实现。
        /// </summary>
        private sealed class FakeInventoryApplicationService : IInventoryApplicationService
        {
            /// <summary>
            /// 根据 SKU 查询库存的测试处理函数。
            /// </summary>
            public Func<string, CancellationToken, Task<InventoryItemResult?>> GetBySkuIdAsyncHandler { get; set; } =
                (_, _) => Task.FromResult<InventoryItemResult?>(new InventoryItemResult(Guid.NewGuid(), "SKU-001", 100, 0, 0, 100));

            /// <summary>
            /// 调整库存的测试处理函数。
            /// </summary>
            public Func<AdjustInventoryCommand, CancellationToken, Task<InventoryItemResult>> AdjustInventoryAsyncHandler { get; set; } =
                (_, _) => Task.FromResult(new InventoryItemResult(Guid.NewGuid(), "SKU-001", 100, 0, 0, 100));

            /// <summary>
            /// 锁定库存的测试处理函数。
            /// </summary>
            public Func<ReserveInventoryCommand, CancellationToken, Task<InventoryReservationResult>> ReserveInventoryAsyncHandler { get; set; } =
                (_, _) => Task.FromResult(new InventoryReservationResult(Guid.NewGuid(), "ERP-001", "SKU-001", 10, "Active", DateTimeOffset.Now));

            /// <summary>
            /// 释放库存预留的测试处理函数。
            /// </summary>
            public Func<Guid, CancellationToken, Task> ReleaseReservationAsyncHandler { get; set; } =
                (_, _) => Task.CompletedTask;

            /// <summary>
            /// 扣减库存预留的测试处理函数。
            /// </summary>
            public Func<Guid, CancellationToken, Task> DeductReservationAsyncHandler { get; set; } =
                (_, _) => Task.CompletedTask;

            /// <summary>
            /// 释放库存预留调用次数。
            /// </summary>
            public int ReleaseReservationCallCount { get; private set; }

            /// <summary>
            /// 根据 SKU 查询库存。
            /// </summary>
            /// <param name="skuId">SKU 标识。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItemResult?> GetBySkuIdAsync(string skuId, CancellationToken cancellationToken = default)
            {
                return GetBySkuIdAsyncHandler(skuId, cancellationToken);
            }

            /// <summary>
            /// 调整库存。
            /// </summary>
            /// <param name="command">调整库存命令。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryItemResult> AdjustInventoryAsync(AdjustInventoryCommand command, CancellationToken cancellationToken = default)
            {
                return AdjustInventoryAsyncHandler(command, cancellationToken);
            }

            /// <summary>
            /// 锁定库存。
            /// </summary>
            /// <param name="command">锁定库存命令。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task<InventoryReservationResult> ReserveInventoryAsync(ReserveInventoryCommand command, CancellationToken cancellationToken = default)
            {
                return ReserveInventoryAsyncHandler(command, cancellationToken);
            }

            /// <summary>
            /// 释放库存预留。
            /// </summary>
            /// <param name="reservationId">库存预留 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            {
                ReleaseReservationCallCount++;
                return ReleaseReservationAsyncHandler(reservationId, cancellationToken);
            }

            /// <summary>
            /// 扣减库存预留。
            /// </summary>
            /// <param name="reservationId">库存预留 ID。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Task DeductReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            {
                return DeductReservationAsyncHandler(reservationId, cancellationToken);
            }
        }
    }
}
