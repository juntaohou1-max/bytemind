using Logistics.Services.Ordering.Api.Application.OutboxMessages;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.Services.Ordering.Api.Controllers
{
    [ApiController]
    [Route("api/outbox-messages")]
    public class OutboxMessagesController : ControllerBase
    {
        private readonly IOutboxMessageQueryService _outboxMessageQueryService;
        private readonly IOutboxMessageOperationService _outboxMessageOperationService;

        public OutboxMessagesController(
            IOutboxMessageQueryService outboxMessageQueryService,
            IOutboxMessageOperationService outboxMessageOperationService)
        {
            _outboxMessageQueryService = outboxMessageQueryService
                ?? throw new ArgumentNullException(nameof(outboxMessageQueryService));
            _outboxMessageOperationService = outboxMessageOperationService
                ?? throw new ArgumentNullException(nameof(outboxMessageOperationService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? status,
            int pageNumber = 1,
            int pageSize = 20,
            string sort = "occurredAtDesc",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _outboxMessageQueryService.GetAllAsync(
                    status,
                    pageNumber,
                    pageSize,
                    sort,
                    cancellationToken);

                return Ok(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var response = await _outboxMessageQueryService.GetByIdAsync(id, cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPost("{id:guid}/retry")]
        public async Task<IActionResult> Retry(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _outboxMessageOperationService.RetryAsync(id, cancellationToken);

                if (response is null)
                {
                    return NotFound();
                }

                return Ok(response);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [HttpPost("retry-failed")]
        public async Task<IActionResult> RetryFailed(
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _outboxMessageOperationService.RetryFailedAsync(
                    take,
                    cancellationToken);

                return Ok(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
