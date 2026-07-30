using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class TestSupplierController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testReqmtService;
        private readonly ITestorProductService _testorProductService;

        public TestSupplierController(
            IMapper mapper,
            ITestRequirementService testReqmtService,
            ITestorProductService testorProductService)
        {
            _mapper = mapper;
            _testReqmtService = testReqmtService;
            _testorProductService = testorProductService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var testCodeOptions = await GetTestorProductSelectListAsync();
            var defaultTestCode = testCodeOptions.FirstOrDefault()?.Value ?? string.Empty;

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var grid = await BuildTestSupplierGridAsync(defaultRequest, defaultTestCode, showRejected: false);

            var viewModel = new TestSupplierViewModel
            {
                SelectedTestCode = defaultTestCode,
                ShowRejected = false,
                TestCodeList = testCodeOptions,
                TestSupplierGrid = grid
            };

            return View(viewModel);
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestSupplierGrid(
            PaginationFilter<string> request, string testCode, bool showRejected)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestSupplierGridAsync(request, testCode, showRejected);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── TEST REQMT CRUD (via PACT API) ────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestReqmt(string testCode)
        {
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            var model = new TestSupplierItem { TestCode = testCode, Active = 1, NoRequired = 0 };

            if (!string.IsNullOrWhiteSpace(testCode))
            {
                var pricing = await _testReqmtService.GetTestReqmtPricingAsync(testCode, null);
                if (pricing.Success && pricing.Data is not null)
                {
                    model.RecUnitPrice = pricing.Data.RecUnitPrice;
                    model.UnitPrice = pricing.Data.RecUnitPrice;
                }
            }

            return PartialView("_AddEditTestSupplier", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestReqmt([FromBody] TestSupplierItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestRequirementDto>(model);
            var result = await _testReqmtService.CreateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Supplier entry created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create entry.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestReqmt(string testCode, string buyer)
        {
            var result = await _testReqmtService.GetTestReqmtByIdAsync(testCode, buyer);
            if (!result.Success)
                return NotFound($"Test Requirement with TestCode '{testCode}' and Buyer '{buyer}' not found.");

            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            var item = _mapper.Map<TestSupplierItem>(result.Data);

            // Populate TestCost and ProjectStatus from the PACT supplier view
            var viewQuery = new QueryParameters<string> { Page = 1, PageSize = 100, Filter = "{}" };
            var viewResult = await _testReqmtService.GetPagedBySupplierTestCodeAsync(viewQuery, testCode, showRejected: true);
            if (viewResult.Success && viewResult.Data != null)
            {
                var viewRow = viewResult.Data.FirstOrDefault(r =>
                    string.Equals(r.Buyer, buyer, StringComparison.OrdinalIgnoreCase));
                if (viewRow != null)
                {
                    item.TestCost = viewRow.TestCost;
                    item.ProjectStatus = viewRow.ProjectStatus;
                }
            }

            return PartialView("_AddEditTestSupplier", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestReqmt([FromBody] TestSupplierItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestRequirementDto>(model);
            var result = await _testReqmtService.UpdateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Supplier entry updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update entry.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestReqmt(string testCode, string buyer)
        {
            var result = await _testReqmtService.DeleteTestReqmtAsync(testCode, buyer);
            return result.Success
                ? Json(new { success = true, message = "Test Supplier entry deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete entry.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> GetTestReqmtPricing(string testCode, string? projectCode = null)
        {
            if (string.IsNullOrWhiteSpace(testCode))
                return Json(new { success = false });

            var result = await _testReqmtService.GetTestReqmtPricingAsync(testCode, projectCode);
            if (!result.Success || result.Data is null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                recUnitPrice = result.Data.RecUnitPrice
            });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<TestSupplierItem>> BuildTestSupplierGridAsync(
            PaginationFilter<string> request, string testCode, bool showRejected)
        {
            var filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testReqmtService.GetPagedBySupplierTestCodeAsync(query, testCode, showRejected);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestSupplierItem>>(response.Data)
                : new List<TestSupplierItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestSupplierItem>
            {
                GridId = "testSupplierGrid",
                Title = "Test Supplier Entries",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = false,
                KeyProperty = "Buyer",
                BindGridUrl = "/FPS/TestSupplier/LoadTestSupplierGrid",
                ExtraFilterMethod = "getTestSupplierExtraFilters",
                AddFunction = "addTestSupplier",
                EditFunction = "editTestSupplier",
                DeleteFunction = "deleteTestSupplier",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestSupplierItem>(),
                Pagination = paginationModel,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true,
                CurrentFilters = filterDict
            };
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(t => new SelectListItem(
                        string.IsNullOrWhiteSpace(t.ItemDescription) ? t.ItemCode : $"{t.ItemCode} – {t.ItemDescription}",
                        t.ItemCode))
                    .ToList()
                : new List<SelectListItem>();
        }
    }
}
