using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using PactMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto;
using StagingMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.StagingMonthlyOutputDto;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MonthlyOutputController : Controller
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _monthlyOutputService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly ITestRequirementService _testRequirementService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyOutputController"/> class.
        /// </summary>
        public MonthlyOutputController(
            IMapper mapper,
            IPactMonthlyOutputService monthlyOutputService,
            IWorkGroupService workGroupService,
            IMonthService monthService,
            IExcelExportService excelExportService,
            ITestCapabilityService testCapabilityService,
            ITestRequirementService testRequirementService)
        {
            _mapper = mapper;
            _monthlyOutputService = monthlyOutputService;
            _workGroupService = workGroupService;
            _monthService = monthService;
            _excelExportService = excelExportService;
            _testCapabilityService = testCapabilityService;
            _testRequirementService = testRequirementService;
        }

        /// <summary>
        /// Displays the monthly output page with initial live and staging grids.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var liveGrid = await BuildLiveGridAsync(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null, null, null, null);

            var stagingGrid = await BuildStagingGridAsync(
                new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 },
                null);

            var viewModel = new MonthlyOutputViewModel
            {
                WorkGroupOptions = await GetWorkGroupOptionsAsync(),
                TestCodeOptions = new List<SelectListItem>(),
                BuyerOptions = new List<SelectListItem>(),
                MonthOptions = await GetMonthOptionsAsync(),
                LiveGrid = liveGrid,
                StagingGrid = stagingGrid,
                LiveTotalVolume = liveGrid.Total,
                StagingTotalVolume = stagingGrid.Total
            };

            return View(viewModel);
        }       

        /// <summary>
        /// Loads the monthly output live grid using supplied filters.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadLiveGrid(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildLiveGridAsync(request, workGroup, testCode, buyer, month);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Loads the monthly output staging grid for the selected filter state.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadStagingGrid(PaginationFilter<string> request, bool? passed)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildStagingGridAsync(request, passed);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Returns test code options for the selected work group.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTestCodesByWorkGroup(string? workGroup)
        {
            var response = await _testCapabilityService.GetPagedByWorkGroupAsync(
                new QueryParameters<string> { Page = -1 },
                string.IsNullOrWhiteSpace(workGroup) ? null : workGroup);

            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var result = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.TestCode))
                .Select(x => x.TestCode!)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new { value = x, text = x })
                .ToList();

            return Json(result);
        }

        /// <summary>
        /// Returns buyer options for the selected test code.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBuyersByTestCode(string? workGroup, string? testCode)
        {
            var response = await _testRequirementService.GetAllActiveAsync();

            if (!response.Success || response.Data == null)
                return Json(Array.Empty<object>());

            var buyers = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.Buyer));

            if (!string.IsNullOrWhiteSpace(testCode))
                buyers = buyers.Where(x => string.Equals(x.TestCode, testCode, StringComparison.OrdinalIgnoreCase));

            var result = buyers
                .DistinctBy(x => x.Buyer)
                .OrderBy(x => x.Buyer)
                .Select(x => new { value = x.Buyer!, text = x.Buyer!, testCode = x.TestCode ?? string.Empty })
                .ToList();

            return Json(result);
        }        

        /// <summary>
        /// Gets a monthly output live record by key and returns the edit partial view.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLiveRecord(string testCode, string buyer, double month, string workGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request data.");

            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();

            var response = await _monthlyOutputService.GetLiveByKeyAsync(testCode, buyer, month, workGroup);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<MonthlyOutputLiveItem>(response.Data);
            return PartialView("_EditMonthlyOutputLive", model);
        }

        /// <summary>
        /// Validates and saves a monthly output live record update.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveLiveRecord([FromBody] MonthlyOutputLiveItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<PactMonthlyOutputDto>(model);

            var keyParts = (model.CompositeKey ?? string.Empty).Split('|');
            dto.OriginalTestCode = keyParts.ElementAtOrDefault(0);
            dto.OriginalBuyer = keyParts.ElementAtOrDefault(1);
            dto.OriginalMonth = double.TryParse(keyParts.ElementAtOrDefault(2), out var km) ? km : 0;
            dto.OriginalWorkGroup = keyParts.ElementAtOrDefault(3);

            var validationResponse = await _monthlyOutputService.ValidateLiveAsync(dto);
            var validationErrors = validationResponse.Data ?? [];
            if (validationErrors.Count > 0)
                return Json(new { success = false, message = "Validation failed.", errors = validationErrors.Select(e => new { field = e.Field, message = e.Message }) });

            var response = await _monthlyOutputService.UpdateLiveAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Monthly output record updated successfully." });

            return Json(new
            {
                success = false,
                message = "Failed to update monthly output record.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Gets a staging monthly output record by id and returns the edit partial view.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStagingRecord(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request data.");

            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();
            ViewBag.TestCodeOptions = await GetAllTestCodesAsync();
            ViewBag.BuyerOptions = await GetAllBuyersAsync();

            var response = await _monthlyOutputService.GetStagingByIdAsync(id);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<StagingMonthlyOutputItem>(response.Data);
            return PartialView("_AddEditStagingMonthlyOutput", model);
        }

        /// <summary>
        /// Returns an empty staging monthly output editor partial for create flow.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AddStagingRecord()
        {
            ViewBag.WorkGroups = await GetWorkGroupOptionsAsync();
            ViewBag.MonthOptions = await GetMonthOptionsAsync();
            ViewBag.TestCodeOptions = await GetAllTestCodesAsync();
            ViewBag.BuyerOptions = await GetAllBuyersAsync();
            return PartialView("_AddEditStagingMonthlyOutput", new StagingMonthlyOutputItem());
        }

        /// <summary>
        /// Creates or updates a staging monthly output record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveStagingRecord([FromBody] StagingMonthlyOutputItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<StagingMonthlyOutputDto>(model);

            if (model.Id != 0)
            {
                dto.Passed = false;
                dto.FailureComments = "This record has been edited since being validated. It will need re-validating.";
            }

            ApiResponseDto<StagingMonthlyOutputDto> response = model.Id == 0
                ? await _monthlyOutputService.CreateStagingAsync(dto)
                : await _monthlyOutputService.UpdateStagingAsync(model.Id, dto);

            if (!response.Success)
                return Json(new
                {
                    success = false,
                    message = "Failed to save staging record.",
                    errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });

            return Json(new
            {
                success = true,
                message = model.Id == 0 ? "Staging record added successfully." : "Staging record updated successfully."
            });
        }

        /// <summary>
        /// Deletes a single staging monthly output record.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteStagingRecord(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request data.");

            var response = await _monthlyOutputService.DeleteStagingAsync(id);
            return Json(new { success = response.Success && response.Data });
        }

        /// <summary>
        /// Deletes all staging monthly output records for the current user.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAllStagingRecords()
        {
            var response = await _monthlyOutputService.DeleteAllStagingByUserAsync();
            return Json(new { success = response.Success && response.Data });
        }

        /// <summary>
        /// Deletes failed staging monthly output records for the current user.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteFailedStagingRecords()
        {
            var response = await _monthlyOutputService.DeleteFailedStagingByUserAsync();
            return Json(new
            {
                success = response.Success && response.Data,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete failed imported records."
            });
        }        

        /// <summary>
        /// Exports staging monthly output records to an Excel file.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportStaging(bool? passed)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request data.");

            var response = await _monthlyOutputService.GetStagingAsync(new QueryParameters<string> { Page = -1 }, passed);
            if (!response.Success || response.Data == null)
                return NotFound();

            var rows = _mapper.Map<List<StagingMonthlyOutputExportItem>>(response.Data);
            var excelBytes = _excelExportService.ExportToExcel(rows, "MonthlyOutput");
            var fileName = $"ExportedOP_{DateTime.Now:ddMMyyyy}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }        

        /// <summary>
        /// Imports a monthly output file into staging.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file, short importType = 1)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request data.");

            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select an Excel file to import." });

            var response = await _monthlyOutputService.ImportMonthlyOutputAsync(file, importType);
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    importedCount = response.Data.ImportedCount,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Import failed." });
        }

        /// <summary>
        /// Validates staged monthly output records.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Validate()
        {
            var response = await _monthlyOutputService.ValidateStagingAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    passedCount = response.Data.PassedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Validation failed." });
        }

        /// <summary>
        /// Moves validated staged monthly output records to live.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MakeLive()
        {
            var response = await _monthlyOutputService.MakeLiveAsync();
            if (response.Success && response.Data != null)
            {
                return Json(new
                {
                    success = true,
                    processedCount = response.Data.ProcessedCount,
                    importedCount = response.Data.ImportedCount,
                    failedCount = response.Data.FailedCount,
                    message = response.Data.Message
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Make live failed.",
                errors = response.Errors
            });
        }
        
        /// <summary>
        /// Builds live grid configuration and data for monthly output.
        /// </summary>
        private async Task<DataGridConfig<MonthlyOutputLiveItem>> BuildLiveGridAsync(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var hasAnyFilter = !string.IsNullOrWhiteSpace(workGroup)
                || !string.IsNullOrWhiteSpace(testCode)
                || !string.IsNullOrWhiteSpace(buyer)
                || month.HasValue;

            var currentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];

            List<MonthlyOutputLiveItem> items;
            decimal total;
            PaginationModel pagination;

            if (!hasAnyFilter)
            {
                items = [];
                total = 0;
                pagination = new PaginationModel
                {
                    TotalRecords = 0,
                    PageNumber = request.Page > 0 ? request.Page : 1,
                    PageSize = request.PageSize > 0 ? request.PageSize : 10
                };
            }
            else
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _monthlyOutputService.GetLiveAsync(query, workGroup, testCode, buyer, month);

                items = response.Success && response.Data != null
                    ? _mapper.Map<List<MonthlyOutputLiveItem>>(response.Data)
                    : [];
                total = response.Total;
                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<MonthlyOutputLiveItem>
            {
                GridId = "monthlyOutputLiveGrid",
                Title = "Monthly Output",
                AllowAdd = false,
                AllowDelete = false,
                ShowCheckboxColumn = false,
                KeyProperty = "CompositeKey",
                EditFunction = "editMonthlyOutputLive",
                BindGridUrl = "/PACT/MonthlyOutput/LoadLiveGrid",
                ExtraFilterMethod = "getMonthlyOutputLiveFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyOutputLiveItem>(null),
                Pagination = pagination,
                CurrentFilters = currentFilters
            };
        }

        /// <summary>
        /// Builds staging grid configuration and data for monthly output.
        /// </summary>
        private async Task<DataGridConfig<StagingMonthlyOutputItem>> BuildStagingGridAsync(
            PaginationFilter<string> request,
            bool? passed)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _monthlyOutputService.GetStagingAsync(query, passed);
            var items = response.Success && response.Data != null
                ? _mapper.Map<List<StagingMonthlyOutputItem>>(response.Data)
                : [];
            var total = response.Total;
            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<StagingMonthlyOutputItem>
            {
                GridId = "monthlyOutputStagingGrid",
                Title = "Imported Output Records",
                AllowExport = false,
                ShowCheckboxColumn = false,
                KeyProperty = "Id",
                AddFunction = "addStagingMonthlyOutput",
                EditFunction = "editStagingMonthlyOutput",
                DeleteFunction = "deleteStagingMonthlyOutput",
                BindGridUrl = "/PACT/MonthlyOutput/LoadStagingGrid",
                ExtraFilterMethod = "getMonthlyOutputStagingFilters",
                Data = items,
                Total = total,
                Columns = GridDataProvider.GetColumnsDefination<StagingMonthlyOutputItem>(null),
                Pagination = pagination,
                CurrentFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? []
            };
        }

        /// <summary>
        /// Gets work group dropdown options.
        /// </summary>
        private async Task<List<SelectListItem>> GetWorkGroupOptionsAsync()
        {
            var response = await _workGroupService.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.WorkGroupName).Select(x => new SelectListItem(x.WorkGroupName, x.WorkGroupName)).ToList()
                : [];
        }

        /// <summary>
        /// Gets month dropdown options.
        /// </summary>
        private async Task<List<SelectListItem>> GetMonthOptionsAsync()
        {
            var response = await _monthService.GetAllMonthsAsync();
            return response.Success && response.Data != null
                ? response.Data.OrderBy(x => x.Monthnumber).Select(x => new SelectListItem(x.Monthnumber.ToString(), x.Monthnumber.ToString())).ToList()
                : [];
        }

        /// <summary>
        /// Gets all distinct test code dropdown options.
        /// </summary>
        private async Task<List<SelectListItem>> GetAllTestCodesAsync()
        {
            var response = await _testCapabilityService.GetPagedByWorkGroupAsync(
                new QueryParameters<string> { Page = -1 }, null);

            if (!response.Success || response.Data == null)
                return [];

            return response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.TestCode))
                .Select(x => x.TestCode!)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem(x, x))
                .ToList();
        }

        /// <summary>
        /// Gets all distinct buyer dropdown options.
        /// </summary>
        private async Task<List<SelectListItem>> GetAllBuyersAsync()
        {
            var response = await _testRequirementService.GetAllActiveAsync();

            if (!response.Success || response.Data == null)
                return [];

            return response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.Buyer))
                .DistinctBy(x => x.Buyer)
                .OrderBy(x => x.Buyer)
                .Select(x => new SelectListItem(x.Buyer!, x.Buyer!))
                .ToList();
        }

    }
}
