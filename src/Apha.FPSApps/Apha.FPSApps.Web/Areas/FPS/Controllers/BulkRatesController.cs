using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Collections;
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
        private readonly IMapper _mapper;

        public const string JobNameFec = BulkRatesJobNames.Fec;
        public const string JobNameStaff = BulkRatesJobNames.Staff;
        public const string JobNameAnimal = BulkRatesJobNames.Animal;

        public BulkRatesController(
            IBulkRatesService bulkRatesService, ILogger<BulkRatesController> logger,
            IFpsYearContext fpsYearContext, IMapper mapper)
        {
            _bulkRatesService = bulkRatesService;
            _logger = logger;
            _fpsYearContext = fpsYearContext;
            _mapper = mapper;
        }

        // US-UI-05: Queue list — all requests, filterable
        public Task<IActionResult> Index(string? jobName = JobNameFec, string? status = null)
            => BuildIndexViewAsync(jobName, status, isLocked: false);

        // Rate-type-locked entry points reached from the sidenav — no "Job type" picker.
        [HttpGet]
        public Task<IActionResult> Fec(string? status = null)
            => BuildIndexViewAsync(JobNameFec, status, isLocked: true);

        [HttpGet]
        public Task<IActionResult> Staff(string? status = null)
            => BuildIndexViewAsync(JobNameStaff, status, isLocked: true);

        [HttpGet]
        public Task<IActionResult> Animal(string? status = null)
            => BuildIndexViewAsync(JobNameAnimal, status, isLocked: true);

        private async Task<IActionResult> BuildIndexViewAsync(string? jobName, string? status, bool isLocked)
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}",
                SortBy = "RequestedAtUtc",
                Descending = true
            };

            var vm = new BulkRatesQueueViewModel
            {
                Grid = await GetBulkRatesGridConfigAsync(defaultRequest, jobName, status),
                JobNameFilter = jobName,
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

        // US-UI-05: Queue grid AJAX reload — paging, sorting, and job/status filtering. FPS year is
        // not client-controllable — it always comes from the app-wide year context (header selector),
        // matching every other year-scoped screen, not a separately overridable grid filter.
        [HttpPost]
        public async Task<IActionResult> LoadBulkRatesGrid(
            PaginationFilter<string> request, string? jobName, string? status)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetBulkRatesGridConfigAsync(request, jobName, status);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<BulkRatesQueueGridItem>> GetBulkRatesGridConfigAsync(
            PaginationFilter<string> request, string? jobName, string? status)
        {
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _bulkRatesService.GetRequestsAsync(queryParameters, jobName, _fpsYearContext.Year, status);

            var items = response.Data != null
                ? _mapper.Map<List<BulkRatesQueueGridItem>>(response.Data)
                : new List<BulkRatesQueueGridItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<BulkRatesQueueGridItem>
            {
                GridId = "bulkRatesGrid",
                Title = "Requests",
                ShowPagination = true,
                KeyProperty = "JobExecutionId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = true,
                ViewFunction = "viewBulkRatesRequest",
                BindGridUrl = "/FPS/BulkRates/LoadBulkRatesGrid",
                ExtraFilterMethod = "getBulkRatesExtraFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<BulkRatesQueueGridItem>(null),
                Pagination = paginationModel
            };
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
            var uploadResult = validationResponse.Success ? validationResponse.Data : null;
            var staging = await GetStagingDataOrEmptyAsync(id);
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            var vm = new BulkRatesDetailViewModel
            {
                Request = requestData,
                CurrentUserEmail = GetCurrentUserEmail(),
                UploadResult = uploadResult,
                FecStagingGrid = BuildStagingGridConfig<BulkRatesStagingFecRowDto, FecStagingGridItem>(
                    staging.FecRows, defaultRequest, "fecStagingGrid", "TestCode",
                    FecStagingGridUrl(id), FecSortSelector,
                    BuildFecValidationLookup(uploadResult), r => r.TestCode),
                AgrupStagingGrid = BuildStagingGridConfig<BulkRatesStagingAgrupRowDto, AgrupStagingGridItem>(
                    staging.AgrupRows, defaultRequest, "agrupStagingGrid", "TestCode",
                    AgrupStagingGridUrl(id), AgrupSortSelector,
                    BuildAgrupValidationLookup(uploadResult), AgrupValidationKey),
                StaffStagingGrid = BuildStagingGridConfig<BulkRatesStagingStaffRowDto, StaffStagingGridItem>(
                    staging.StaffRows, defaultRequest, "staffStagingGrid", "PcGrade",
                    StaffStagingGridUrl(id), StaffSortSelector),
                AnimalStagingGrid = BuildStagingGridConfig<BulkRatesStagingAnimalRowDto, AnimalStagingGridItem>(
                    staging.AnimalRows, defaultRequest, "animalStagingGrid", "AnimalType",
                    AnimalStagingGridUrl(id), AnimalSortSelector)
            };
            return View(vm);
        }

        // ── Staging grids (Detail page) — AJAX reload: paging + sorting only, no server-side
        // filtering (matches the queue grid's IsFilterable=false columns). Data for a single
        // request is already fully fetched via GetStagingDataAsync — small enough (bounded by
        // one Excel upload) that in-memory sort/page here is proportionate, and avoids adding
        // paging support to the FPS API's staging endpoint.

        [HttpPost]
        public async Task<IActionResult> LoadFecStagingGrid(PaginationFilter<string> request, Guid jobExecutionId)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var staging = await GetStagingDataOrEmptyAsync(jobExecutionId);
            var validationResponse = await _bulkRatesService.GetValidationResultsAsync(jobExecutionId);
            var gridConfig = BuildStagingGridConfig<BulkRatesStagingFecRowDto, FecStagingGridItem>(
                staging.FecRows, request, "fecStagingGrid", "TestCode",
                FecStagingGridUrl(jobExecutionId), FecSortSelector,
                BuildFecValidationLookup(validationResponse.Success ? validationResponse.Data : null), r => r.TestCode);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAgrupStagingGrid(PaginationFilter<string> request, Guid jobExecutionId)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var staging = await GetStagingDataOrEmptyAsync(jobExecutionId);
            var validationResponse = await _bulkRatesService.GetValidationResultsAsync(jobExecutionId);
            var gridConfig = BuildStagingGridConfig<BulkRatesStagingAgrupRowDto, AgrupStagingGridItem>(
                staging.AgrupRows, request, "agrupStagingGrid", "TestCode",
                AgrupStagingGridUrl(jobExecutionId), AgrupSortSelector,
                BuildAgrupValidationLookup(validationResponse.Success ? validationResponse.Data : null), AgrupValidationKey);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffStagingGrid(PaginationFilter<string> request, Guid jobExecutionId)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var staging = await GetStagingDataOrEmptyAsync(jobExecutionId);
            var gridConfig = BuildStagingGridConfig<BulkRatesStagingStaffRowDto, StaffStagingGridItem>(
                staging.StaffRows, request, "staffStagingGrid", "PcGrade",
                StaffStagingGridUrl(jobExecutionId), StaffSortSelector);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalStagingGrid(PaginationFilter<string> request, Guid jobExecutionId)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var staging = await GetStagingDataOrEmptyAsync(jobExecutionId);
            var gridConfig = BuildStagingGridConfig<BulkRatesStagingAnimalRowDto, AnimalStagingGridItem>(
                staging.AnimalRows, request, "animalStagingGrid", "AnimalType",
                AnimalStagingGridUrl(jobExecutionId), AnimalSortSelector);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<BulkRatesStagingDataDto> GetStagingDataOrEmptyAsync(Guid jobExecutionId)
        {
            var response = await _bulkRatesService.GetStagingDataAsync(jobExecutionId);
            return response.Success && response.Data != null ? response.Data : new BulkRatesStagingDataDto();
        }

        private static string FecStagingGridUrl(Guid id) => $"/FPS/BulkRates/LoadFecStagingGrid?jobExecutionId={id}";
        private static string AgrupStagingGridUrl(Guid id) => $"/FPS/BulkRates/LoadAgrupStagingGrid?jobExecutionId={id}";
        private static string StaffStagingGridUrl(Guid id) => $"/FPS/BulkRates/LoadStaffStagingGrid?jobExecutionId={id}";
        private static string AnimalStagingGridUrl(Guid id) => $"/FPS/BulkRates/LoadAnimalStagingGrid?jobExecutionId={id}";

        private static string AgrupValidationKey(BulkRatesStagingAgrupRowDto r) => $"{r.TestCode}|{r.Buyer}";

        // Row-level findings are matched onto a staged row by business key (TestCode for FEC,
        // TestCode+Buyer for AGRUP) so they can render inline in the grid instead of a separate
        // modal. Findings with no TestCode (file-parse errors, DR-UI-04 request-level findings)
        // aren't in either lookup and are listed on the page instead — see Detail.cshtml.
        private static Dictionary<string, string> BuildFecValidationLookup(BulkRatesUploadResultDto? uploadResult)
        {
            if (uploadResult == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return uploadResult.ValidationErrors
                .Where(e => !string.IsNullOrEmpty(e.TestCode) && string.IsNullOrEmpty(e.Buyer))
                .GroupBy(e => e.TestCode!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g.Select(e => $"{e.Severity}: {e.ValidationMessage}")),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> BuildAgrupValidationLookup(BulkRatesUploadResultDto? uploadResult)
        {
            if (uploadResult == null) return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return uploadResult.ValidationErrors
                .Where(e => !string.IsNullOrEmpty(e.TestCode) && !string.IsNullOrEmpty(e.Buyer))
                .GroupBy(e => $"{e.TestCode}|{e.Buyer}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join("; ", g.Select(e => $"{e.Severity}: {e.ValidationMessage}")),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Func<BulkRatesStagingFecRowDto, object?> FecSortSelector(string? sortBy) => sortBy?.ToLowerInvariant() switch
        {
            "status" => r => r.Status,
            "unitpricevla" => r => r.UnitPriceVla,
            "defraunitprice" => r => r.DefraUnitPrice,
            "fecnewrate" => r => r.FecNewRate,
            "itemdescription" => r => r.ItemDescription,
            "shortdescription" => r => r.ShortDescription,
            "owner" => r => r.Owner,
            "comments" => r => r.Comments,
            _ => r => r.TestCode
        };

        private static Func<BulkRatesStagingAgrupRowDto, object?> AgrupSortSelector(string? sortBy) => sortBy?.ToLowerInvariant() switch
        {
            "status" => r => r.Status,
            "buyer" => r => r.Buyer,
            "agrup" => r => r.Agrup,
            "agrupnew" => r => r.AgrupNew,
            "norequired" => r => r.NoRequired,
            "datecreated" => r => r.DateCreated,
            "active" => r => r.Active,
            "comments" => r => r.Comments,
            _ => r => r.TestCode
        };

        private static Func<BulkRatesStagingStaffRowDto, object?> StaffSortSelector(string? sortBy) => sortBy?.ToLowerInvariant() switch
        {
            "status" => r => r.Status,
            "payrate" => r => r.PayRate,
            "payratenew" => r => r.PayRateNew,
            "npr" => r => r.Npr,
            "nprnew" => r => r.NprNew,
            "ohr" => r => r.Ohr,
            "ohrnew" => r => r.OhrNew,
            _ => r => r.PcGrade
        };

        private static Func<BulkRatesStagingAnimalRowDto, object?> AnimalSortSelector(string? sortBy) => sortBy?.ToLowerInvariant() switch
        {
            "status" => r => r.Status,
            "species" => r => r.Species,
            "securitylevel" => r => r.SecurityLevel,
            "dailyrate" => r => r.DailyRate,
            "dailyratenew" => r => r.DailyRateNew,
            "defradailyrate" => r => r.DefraDailyRate,
            "defradailyratenew" => r => r.DefraDailyRateNew,
            "planbyweek" => r => r.PlanByWeek,
            _ => r => r.AnimalType
        };

        private DataGridConfig<TItem> BuildStagingGridConfig<TRow, TItem>(
            IReadOnlyList<TRow> rows, PaginationFilter<string> request,
            string gridId, string keyProperty, string bindGridUrl,
            Func<string?, Func<TRow, object?>> sortKeySelector,
            IReadOnlyDictionary<string, string>? validationLookup = null,
            Func<TRow, string?>? validationKeySelector = null)
            where TItem : class
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
            var keySelector = sortKeySelector(request.SortBy);

            var ordered = request.Descending
                ? rows.OrderByDescending(keySelector, NullSafeComparer.Instance)
                : rows.OrderBy(keySelector, NullSafeComparer.Instance);

            var pageRows = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var items = _mapper.Map<List<TItem>>(pageRows);

            // Attach the matching validation message(s) to each mapped row, keyed by business key
            // (TestCode for FEC, TestCode+Buyer for AGRUP) — see BuildFecValidationLookup /
            // BuildAgrupValidationLookup. Only grid items that opt in via IHasValidationSummary
            // are touched, so this is a no-op for Staff/Animal grids.
            if (validationLookup is { Count: > 0 } && validationKeySelector != null)
            {
                for (var i = 0; i < pageRows.Count; i++)
                {
                    if (items[i] is not IHasValidationSummary target) break;
                    var key = validationKeySelector(pageRows[i]);
                    if (key != null && validationLookup.TryGetValue(key, out var summary))
                        target.ValidationSummary = summary;
                }
            }

            return new DataGridConfig<TItem>
            {
                GridId = gridId,
                ShowPagination = true,
                KeyProperty = keyProperty,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = false,
                BindGridUrl = bindGridUrl,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TItem>(null),
                Pagination = new PaginationModel
                {
                    TotalRecords = rows.Count,
                    PageNumber = page,
                    PageSize = pageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                }
            };
        }

        // OrderBy/OrderByDescending need an IComparer<object?> since staging columns span
        // strings, decimals, dates and bools — Comparer.Default compares same-type boxed
        // IComparable values correctly regardless of the underlying type.
        private sealed class NullSafeComparer : IComparer<object?>
        {
            public static readonly NullSafeComparer Instance = new();
            public int Compare(object? x, object? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                return Comparer.Default.Compare(x, y);
            }
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

        // DR-UI-01: Download FEC test data snapshot atomically tied to a specific request.
        [HttpGet]
        public async Task<IActionResult> DownloadTestDataForRequest(Guid id)
        {
            try
            {
                var bytes = await _bulkRatesService.DownloadFecTestDataForRequestAsync(id);
                var fileName = $"FEC_TestRates_{id}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkRates DownloadTestDataForRequest failed for request {Id}", id);
                TempData["ErrorMessage"] = "The FEC test data could not be downloaded. Please try again.";
                return RedirectToAction(nameof(Detail), new { id });
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
