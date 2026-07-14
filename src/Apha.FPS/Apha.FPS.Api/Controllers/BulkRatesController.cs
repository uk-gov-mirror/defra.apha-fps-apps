using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// Controller for the Bulk Rates Update request lifecycle (Phase 3, US-API-01 through US-API-14).
    /// All operations require the API-FPSAdmin role. User identity is read from the
    /// authenticated token via <see cref="IFpsRequestContext.UserEmailId"/>.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "API-FPSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/bulk-rates")]
    public class BulkRatesController : ControllerBase
    {
        private readonly IBulkRatesRequestService _service;
        private readonly IFpsRequestContext _requestContext;

        public BulkRatesController(IBulkRatesRequestService service, IFpsRequestContext requestContext)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        /// <summary>US-API-01: Create a new Bulk Rates request in Initiated status.</summary>
        [HttpPost("requests")]
        public async Task<ActionResult<BulkRatesRequestDto>> CreateRequestAsync(
            [FromBody] CreateBulkRatesReq req,
            CancellationToken ct)
        {
            var result = await _service.CreateRequestAsync(
                req.JobName, req.FpsYear, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>US-API-02/03/05: Upload (or re-upload) an Excel file, replacing previous staging and re-running validation.</summary>
        [HttpPost("requests/{id:guid}/upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BulkRatesUploadResultDto>> UploadFileAsync(
            Guid id, IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("A non-empty file is required.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);

            var result = await _service.UploadFileAsync(
                id, ms.ToArray(), file.FileName, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>US-API-04: Retrieve structured validation results for a request.</summary>
        [HttpGet("requests/{id:guid}/validation")]
        public async Task<ActionResult<BulkRatesUploadResultDto>> GetValidationResultsAsync(
            Guid id, CancellationToken ct)
        {
            var result = await _service.GetValidationResultsAsync(
                id, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>US-API-06/12/13: Release a fully-valid request for approval.</summary>
        [HttpPost("requests/{id:guid}/release")]
        public async Task<ActionResult<BulkRatesRequestDto>> ReleaseForApprovalAsync(
            Guid id, CancellationToken ct)
        {
            var result = await _service.ReleaseForApprovalAsync(
                id, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>US-API-07/09/10/12/13: Approve and publish EventBridge trigger.</summary>
        [HttpPost("requests/{id:guid}/approve")]
        public async Task<ActionResult<BulkRatesRequestDto>> ApproveAsync(
            Guid id, CancellationToken ct)
        {
            var result = await _service.ApproveAsync(
                id, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>US-API-08/13: Reject with mandatory reason.</summary>
        [HttpPost("requests/{id:guid}/reject")]
        public async Task<ActionResult<BulkRatesRequestDto>> RejectAsync(
            Guid id, [FromBody] RejectBulkRatesReq req, CancellationToken ct)
        {
            var result = await _service.RejectAsync(
                id, _requestContext.UserEmailId, req.Reason, ct);
            return Ok(result);
        }

        /// <summary>US-API-14/13: Cancel an Initiated or Rejected request (initiator only).</summary>
        [HttpPost("requests/{id:guid}/cancel")]
        public async Task<ActionResult<BulkRatesRequestDto>> CancelAsync(
            Guid id, [FromBody] CancelBulkRatesReq req, CancellationToken ct)
        {
            var result = await _service.CancelAsync(
                id, _requestContext.UserEmailId, req.Reason, ct);
            return Ok(result);
        }

        /// <summary>US-API-11: Get full request detail including log history.</summary>
        [HttpGet("requests/{id:guid}")]
        public async Task<ActionResult<BulkRatesRequestDto>> GetRequestAsync(
            Guid id, CancellationToken ct)
        {
            var result = await _service.GetRequestAsync(id, ct);
            if (result == null)
                return NotFound($"Bulk Rates request {id} not found.");
            return Ok(result);
        }

        /// <summary>US-API-11: List requests, optionally filtered by job name, year and status.</summary>
        [HttpGet("requests")]
        public async Task<ActionResult> GetRequestsAsync(
            [FromQuery] string? jobName,
            [FromQuery] int? fpsYear,
            [FromQuery] string? status,
            CancellationToken ct)
        {
            var result = await _service.GetRequestsAsync(jobName, fpsYear, status, ct);
            return Ok(result);
        }
    }

    // ── Request body records ─────────────────────────────────────────────────────

    /// <param name="JobName">Job type name matching fps.job_master.jobname (e.g. "FEC", "Animal", "Staff").</param>
    /// <param name="FpsYear">The FPS year the bulk rates update applies to.</param>
    public record CreateBulkRatesReq(string JobName, int FpsYear);

    /// <param name="Reason">Mandatory rejection reason visible to the initiator.</param>
    public record RejectBulkRatesReq(string Reason);

    /// <param name="Reason">Optional cancellation reason.</param>
    public record CancelBulkRatesReq(string? Reason);
}
