using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class BulkRatesController : Controller
    {
        private readonly IBulkRatesService _bulkRatesService;
        private readonly ILogger<BulkRatesController> _logger;

        public const string JobNameFec = "BulkTestRatesUpdate";
        public const string JobNameStaff = "BulkStaffRatesUpdate";
        public const string JobNameAnimal = "BulkAnimalRatesUpdate";

        public BulkRatesController(IBulkRatesService bulkRatesService, ILogger<BulkRatesController> logger)
        {
            _bulkRatesService = bulkRatesService;
            _logger = logger;
        }

        // US-UI-05: Queue list — all requests, filterable
        public async Task<IActionResult> Index(string? jobName = null, int? fpsYear = null, string? status = null)
        {
            var response = await _bulkRatesService.GetRequestsAsync(jobName, fpsYear, status);
            var vm = new BulkRatesQueueViewModel
            {
                Entries = response.Data ?? [],
                JobNameFilter = jobName,
                FpsYearFilter = fpsYear,
                StatusFilter = status
            };
            return View(vm);
        }

        // US-UI-01: Create request — GET form
        [HttpGet]
        public IActionResult Create(string jobName = JobNameFec)
        {
            return View(new BulkRatesCreateViewModel
            {
                JobName = jobName,
                FpsYear = DateTime.UtcNow.Year
            });
        }

        // US-UI-01: Create request — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string jobName, int fpsYear)
        {
            var response = await _bulkRatesService.CreateRequestAsync(jobName, fpsYear);
            if (response.Success && response.Data != null)
                return Json(new { success = true, id = response.Data.Entry.JobQueueId });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create request.";
            _logger.LogWarning("BulkRates Create failed: {Message}", msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-07: Request detail page — GET
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var requestResponse = await _bulkRatesService.GetRequestAsync(id);
            if (!requestResponse.Success || requestResponse.Data == null)
                return NotFound();

            var validationResponse = await _bulkRatesService.GetValidationResultsAsync(id);

            var vm = new BulkRatesDetailViewModel
            {
                Request = requestResponse.Data,
                CurrentUserEmail = GetCurrentUserEmail(),
                UploadResult = validationResponse.Success ? validationResponse.Data : null
            };
            return View(vm);
        }

        // US-UI-02/03/05: Upload (or re-upload) Excel file — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Guid id, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return Json(new { success = false, message = "Please select a file before uploading." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var response = await _bulkRatesService.UploadFileAsync(id, bytes, file.FileName);
            if (response.Success)
                return Json(new { success = true, data = response.Data });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Upload failed.";
            _logger.LogWarning("BulkRates Upload failed for {Id}: {Message}", id, msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-04: Release for approval — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(Guid id)
        {
            var response = await _bulkRatesService.ReleaseForApprovalAsync(id);
            if (response.Success)
                return Json(new { success = true });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Release failed.";
            _logger.LogWarning("BulkRates Release failed for {Id}: {Message}", id, msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-05: Approve — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var response = await _bulkRatesService.ApproveAsync(id);
            if (response.Success)
                return Json(new { success = true });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Approval failed.";
            _logger.LogWarning("BulkRates Approve failed for {Id}: {Message}", id, msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-06: Reject with mandatory reason — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return Json(new { success = false, message = "Rejection reason is required." });

            var response = await _bulkRatesService.RejectAsync(id, reason);
            if (response.Success)
                return Json(new { success = true });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Rejection failed.";
            _logger.LogWarning("BulkRates Reject failed for {Id}: {Message}", id, msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-08: Cancel — POST (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id, string? reason)
        {
            var response = await _bulkRatesService.CancelAsync(id, reason);
            if (response.Success)
                return Json(new { success = true });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Cancellation failed.";
            _logger.LogWarning("BulkRates Cancel failed for {Id}: {Message}", id, msg);
            return Json(new { success = false, message = msg });
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private string GetCurrentUserEmail()
            => User.FindFirst("preferred_username")?.Value
               ?? User.FindFirst(ClaimTypes.Email)?.Value
               ?? User.Identity?.Name
               ?? string.Empty;
    }
}
