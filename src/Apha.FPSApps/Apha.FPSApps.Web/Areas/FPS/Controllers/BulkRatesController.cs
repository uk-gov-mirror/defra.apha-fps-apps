using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
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
        private readonly IFpsYearContext _fpsYearContext;

        public const string JobNameFec = BulkRatesJobNames.Fec;
        public const string JobNameStaff = BulkRatesJobNames.Staff;
        public const string JobNameAnimal = BulkRatesJobNames.Animal;

        public BulkRatesController(IBulkRatesService bulkRatesService, ILogger<BulkRatesController> logger, IFpsYearContext fpsYearContext)
        {
            _bulkRatesService = bulkRatesService;
            _logger = logger;
            _fpsYearContext = fpsYearContext;
        }

        // US-UI-05: Queue list — all requests, filterable
        public Task<IActionResult> Index(string? jobName = JobNameFec, int? fpsYear = null, string? status = null)
            => BuildIndexViewAsync(jobName, fpsYear, status, isLocked: false);

        // Rate-type-locked entry points reached from the sidenav — no "Job type" picker.
        [HttpGet]
        public Task<IActionResult> Fec(int? fpsYear = null, string? status = null)
            => BuildIndexViewAsync(JobNameFec, fpsYear, status, isLocked: true);

        [HttpGet]
        public Task<IActionResult> Staff(int? fpsYear = null, string? status = null)
            => BuildIndexViewAsync(JobNameStaff, fpsYear, status, isLocked: true);

        [HttpGet]
        public Task<IActionResult> Animal(int? fpsYear = null, string? status = null)
            => BuildIndexViewAsync(JobNameAnimal, fpsYear, status, isLocked: true);

        private async Task<IActionResult> BuildIndexViewAsync(string? jobName, int? fpsYear, string? status, bool isLocked)
        {
            fpsYear ??= _fpsYearContext.Year;

            var response = await _bulkRatesService.GetRequestsAsync(jobName, fpsYear, status);
            var vm = new BulkRatesQueueViewModel
            {
                Entries = response.Data ?? [],
                JobNameFilter = jobName,
                FpsYearFilter = fpsYear,
                StatusFilter = status,
                CurrentUserEmail = GetCurrentUserEmail(),
                IsJobNameLocked = isLocked
            };

            if (!string.IsNullOrEmpty(jobName))
            {
                var activeResponse = await _bulkRatesService.GetActiveRequestAsync(jobName);
                vm.ActiveRequest = activeResponse.Success ? activeResponse.Data : null;
            }

            return View("Index", vm);
        }

        // US-UI-01: Create request — GET form
        [HttpGet]
        public async Task<IActionResult> Create(string jobName = JobNameFec)
        {
            var activeResponse = await _bulkRatesService.GetActiveRequestAsync(jobName);
            if (activeResponse.Success && activeResponse.Data != null)
            {
                TempData["ErrorMessage"] =
                    $"An active {jobName} request already exists (Status={activeResponse.Data.Status}, " +
                    $"Requested by {activeResponse.Data.RequestedBy}). Complete, reject, or cancel it before creating a new one.";
                return RedirectToAction(nameof(Detail), new { id = activeResponse.Data.JobExecutionId });
            }

            return View(new BulkRatesCreateViewModel
            {
                JobName = jobName,
                FpsYear = _fpsYearContext.Year
            });
        }

        // US-UI-01: Create request — POST (AJAX)
        // FPS year is not user-selectable: the posted fpsYear is ignored and the header
        // year-context value is used instead, enforced server-side regardless of client input.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string jobName, int fpsYear)
        {
            var response = await _bulkRatesService.CreateRequestAsync(jobName, _fpsYearContext.Year);
            if (response.Success && response.Data != null)
                return Json(new { success = true, id = response.Data.Entry.JobExecutionId });

            var msg = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create request.";
            _logger.LogWarning("BulkRates Create failed: {Message}", msg);
            return Json(new { success = false, message = msg });
        }

        // US-UI-07: Request detail page — GET
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            BulkRatesRequestDetailDto? requestData;
            try
            {
                var requestResponse = await _bulkRatesService.GetRequestAsync(id);
                if (!requestResponse.Success || requestResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "The requested bulk rates entry could not be found.";
                    return RedirectToAction(nameof(Index));
                }
                requestData = requestResponse.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates Detail failed for {Id}", id);
                TempData["ErrorMessage"] = "Unable to load the request details. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            var validationResponse = await _bulkRatesService.GetValidationResultsAsync(id);
            var stagingResponse = await _bulkRatesService.GetStagingDataAsync(id);

            var vm = new BulkRatesDetailViewModel
            {
                Request = requestData,
                CurrentUserEmail = GetCurrentUserEmail(),
                UploadResult = validationResponse.Success ? validationResponse.Data : null,
                StagingData = stagingResponse.Success ? stagingResponse.Data : null
            };
            return View(vm);
        }

        // Download the staged (not-yet-approved) FEC/AGRUP rows for a request as Excel
        [HttpGet]
        public async Task<IActionResult> DownloadStagingData(Guid id)
        {
            try
            {
                var bytes = await _bulkRatesService.DownloadStagingDataAsync(id);
                var fileName = $"BulkRates_Staging_{id}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates DownloadStagingData failed for {Id}", id);
                TempData["ErrorMessage"] = "The staging data could not be downloaded. Please try again.";
                return RedirectToAction(nameof(Detail), new { id });
            }
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

        // Download current year FEC test data as Excel
        [HttpGet]
        public async Task<IActionResult> DownloadTestData(int fpsYear)
        {
            try
            {
                var bytes = await _bulkRatesService.DownloadFecTestDataAsync(fpsYear);
                var fileName = $"FEC_TestRates_{fpsYear}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates DownloadTestData failed for year {Year}", fpsYear);
                TempData["ErrorMessage"] = "The FEC test data could not be downloaded. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Download current year Staff test data as Excel
        [HttpGet]
        public async Task<IActionResult> DownloadStaffTestData(int fpsYear)
        {
            try
            {
                var bytes = await _bulkRatesService.DownloadStaffTestDataAsync(fpsYear);
                var fileName = $"Staff_TestRates_{fpsYear}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates DownloadStaffTestData failed for year {Year}", fpsYear);
                TempData["ErrorMessage"] = "The Staff test data could not be downloaded. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Download current year Animal test data as Excel
        [HttpGet]
        public async Task<IActionResult> DownloadAnimalTestData(int fpsYear)
        {
            try
            {
                var bytes = await _bulkRatesService.DownloadAnimalTestDataAsync(fpsYear);
                var fileName = $"Animal_TestRates_{fpsYear}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates DownloadAnimalTestData failed for year {Year}", fpsYear);
                TempData["ErrorMessage"] = "The Animal test data could not be downloaded. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private string GetCurrentUserEmail()
            => User.FindFirst("preferred_username")?.Value
               ?? User.FindFirst(ClaimTypes.Email)?.Value
               ?? User.Identity?.Name
               ?? string.Empty;
    }
}
