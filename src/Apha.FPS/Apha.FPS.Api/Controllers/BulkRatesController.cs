using Apha.Common.Contracts;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// Controller for the Bulk Rates Update request lifecycle (Phase 3).
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

        /// <summary>Create a new Bulk Rates request in Initiated status.</summary>
        [HttpPost("requests")]
        public async Task<ActionResult<BulkRatesRequestDto>> CreateRequestAsync(
            [FromBody] CreateBulkRatesReq req,
            CancellationToken ct)
        {
            var result = await _service.CreateRequestAsync(
                req.JobName, req.FpsYear, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>Upload (or re-upload) an Excel file, replacing previous staging and re-running validation.</summary>
        [HttpPost("requests/{jobExecutionId:guid}/upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BulkRatesUploadResultDto>> UploadFileAsync(
            Guid jobExecutionId, IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("A non-empty file is required.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);

            var result = await _service.UploadFileAsync(
                jobExecutionId, ms.ToArray(), file.FileName, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>Retrieve structured validation results for a request.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/validation")]
        public async Task<ActionResult<BulkRatesUploadResultDto>> GetValidationResultsAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var result = await _service.GetValidationResultsAsync(
                jobExecutionId, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>Release a fully-valid request for approval.</summary>
        [HttpPost("requests/{jobExecutionId:guid}/release")]
        public async Task<ActionResult<BulkRatesRequestDto>> ReleaseForApprovalAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var result = await _service.ReleaseForApprovalAsync(
                jobExecutionId, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>Approve and publish EventBridge trigger.</summary>
        [HttpPost("requests/{jobExecutionId:guid}/approve")]
        public async Task<ActionResult<BulkRatesRequestDto>> ApproveAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var result = await _service.ApproveAsync(
                jobExecutionId, _requestContext.UserEmailId, ct);
            return Ok(result);
        }

        /// <summary>Reject with mandatory reason.</summary>
        [HttpPost("requests/{jobExecutionId:guid}/reject")]
        public async Task<ActionResult<BulkRatesRequestDto>> RejectAsync(
            Guid jobExecutionId, [FromBody] RejectBulkRatesReq req, CancellationToken ct)
        {
            var result = await _service.RejectAsync(
                jobExecutionId, _requestContext.UserEmailId, req.Reason, ct);
            return Ok(result);
        }

        /// <summary>Cancel an Initiated or Rejected request (initiator only).</summary>
        [HttpPost("requests/{jobExecutionId:guid}/cancel")]
        public async Task<ActionResult<BulkRatesRequestDto>> CancelAsync(
            Guid jobExecutionId, [FromBody] CancelBulkRatesReq req, CancellationToken ct)
        {
            var result = await _service.CancelAsync(
                jobExecutionId, _requestContext.UserEmailId, req.Reason, ct);
            return Ok(result);
        }

        /// <summary>Get full request detail including log history.</summary>
        [HttpGet("requests/{jobExecutionId:guid}")]
        public async Task<ActionResult<BulkRatesRequestDto>> GetRequestAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var result = await _service.GetRequestAsync(jobExecutionId, ct);
            if (result == null)
                return NotFound($"Bulk Rates request {jobExecutionId} not found.");
            return Ok(result);
        }

        /// <summary>Server-side paged/sorted list, optionally filtered by job name, year and status.</summary>
        [HttpGet("requests")]
        public async Task<ActionResult> GetRequestsAsync(
            [FromQuery] string? jobName,
            [FromQuery] int? fpsYear,
            [FromQuery] string? status,
            [FromQuery] QueryParameters<string> query,
            CancellationToken ct)
        {
            var result = await _service.GetRequestsAsync(jobName, fpsYear, status, query, ct);
            return Ok(new PaginationRes<BulkRatesQueueEntry>
            {
                Data = result.Data,
                PaginationData = new Pagination
                {
                    PageNumber = result.PaginationData.PageNumber,
                    PageSize = result.PaginationData.PageSize,
                    TotalPages = result.PaginationData.TotalPages,
                    TotalRecords = result.PaginationData.TotalRecords
                }
            });
        }

        /// <summary>Returns the currently active (blocking-status) request for a job name, or null if none exists.</summary>
        [HttpGet("requests/active")]
        public async Task<ActionResult<BulkRatesQueueEntry?>> GetActiveRequestAsync(
            [FromQuery] string jobName, CancellationToken ct)
        {
            var result = await _service.GetActiveRequestAsync(jobName, ct);
            return Ok(result);
        }

        /// <summary>Returns the staged FEC/AGRUP rows for a request's staging grids, classified against live data.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/staging")]
        public async Task<ActionResult<BulkRatesStagingDataDto>> GetStagingDataAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var result = await _service.GetStagingDataAsync(jobExecutionId, ct);
            return Ok(result);
        }

        /// <summary>Exports the staged (not-yet-approved) FEC/AGRUP rows for a request as an Excel file.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/staging/export")]
        public async Task<IActionResult> ExportStagingDataAsync(Guid jobExecutionId, CancellationToken ct)
        {
            var bytes = await _service.ExportStagingDataAsync(jobExecutionId, ct);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BulkRates_Staging_{jobExecutionId}.xlsx");
        }

        /// <summary>Atomically snapshot live FEC/AGRUP data for a specific request and return the workbook.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/download")]
        public async Task<IActionResult> DownloadFecTestDataAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var bytes = await _service.DownloadFecTestDataAsync(jobExecutionId, ct);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FEC_TestRates_{jobExecutionId}.xlsx");
        }

        /// <summary>Request-scoped Staff workbook download, parity with DownloadFecTestDataAsync.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/download/staff")]
        public async Task<IActionResult> DownloadStaffTestDataForRequestAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var bytes = await _service.DownloadStaffTestDataAsync(jobExecutionId, ct);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Staff_TestRates_{jobExecutionId}.xlsx");
        }

        /// <summary>As DownloadStaffTestDataForRequestAsync, for Animal.</summary>
        [HttpGet("requests/{jobExecutionId:guid}/download/animal")]
        public async Task<IActionResult> DownloadAnimalTestDataForRequestAsync(
            Guid jobExecutionId, CancellationToken ct)
        {
            var bytes = await _service.DownloadAnimalTestDataAsync(jobExecutionId, ct);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Animal_TestRates_{jobExecutionId}.xlsx");
        }

        /// <summary>Export current FEC test rates for a given FPS year as an Excel file.</summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportFecTestDataAsync(
            [FromQuery] int fpsYear, CancellationToken ct)
        {
            var bytes = await _service.ExportFecTestDataAsync(fpsYear, ct);
            var fileName = $"FEC_TestRates_{fpsYear}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>Export current Staff rates for a given FPS year as an Excel file.</summary>
        [HttpGet("export/staff")]
        public async Task<IActionResult> ExportStaffTestDataAsync(
            [FromQuery] int fpsYear, CancellationToken ct)
        {
            var bytes = await _service.ExportStaffTestDataAsync(fpsYear, ct);
            var fileName = $"Staff_TestRates_{fpsYear}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>Export current Animal rates for a given FPS year as an Excel file.</summary>
        [HttpGet("export/animal")]
        public async Task<IActionResult> ExportAnimalTestDataAsync(
            [FromQuery] int fpsYear, CancellationToken ct)
        {
            var bytes = await _service.ExportAnimalTestDataAsync(fpsYear, ct);
            var fileName = $"Animal_TestRates_{fpsYear}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
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
