using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
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
    public class TestPlanJobController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestorProductService _testorProductService;

        public TestPlanJobController(
            IMapper mapper,
            ITestRequirementService testRequirementService,
            ITestorProductService testorProductService)
        {
            _mapper = mapper;
            _testRequirementService = testRequirementService;
            _testorProductService = testorProductService;
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestPlanGrid(PaginationFilter<string> request, string? jobCode = null, string? title = null)
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            QueryParameters<string> query = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<TestRequirementDto>> response =
                await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, jobCode ?? string.Empty);

            List<TestPlanItem> items = response.Success && response.Data != null
                ? _mapper.Map<List<TestPlanItem>>(response.Data)
                : new List<TestPlanItem>();

            PaginationModel paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridTitle = title ?? "Test Purchase Plan";
            var testPlanGrid = new DataGridConfig<TestPlanItem>
            {
                GridId = "testPlanGrid",
                Title = gridTitle,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowDelete = true,
                KeyProperty = "TestCode",
                AddFunction = "addTestPlan",
                EditFunction = "editTestPlan",
                DeleteFunction = "deleteTestPlan",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = $"/FPS/TestPlanJob/LoadTestPlanGrid?title={Uri.EscapeDataString(gridTitle)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestPlanItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", testPlanGrid);
        }

        // ── CRUD: CREATE ──────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new TestPlanItem { Active = 1, NoRequired = 0 };
            await PopulateTestCodeDropdownAsync(model);
            return PartialView("_AddEditTestPlan", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TestPlanItem item)
        {
            if (!ModelState.IsValid)
            {
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
            }

            TestRequirementDto dto = _mapper.Map<TestRequirementDto>(item);
            ApiResponseDto<TestRequirementDto> result = await _testRequirementService.CreateTestReqmtAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = "Test plan item created successfully." });
            }

            var duplicateError = (result.Errors ?? new List<ApiErrorDto>())
                .FirstOrDefault(e => IsDuplicateError(e));

            if (duplicateError != null)
            {
                const string friendlyMessage = "This test code has already been added to this project. Please update the existing entry instead.";
                return Json(new
                {
                    success = false,
                    message = friendlyMessage,
                    errors = new[] { new { field = string.Empty, message = friendlyMessage } }
                });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create test plan item.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD: EDIT ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Edit(string testCode, string buyer)
        {
            ApiResponseDto<TestRequirementDto> result =
                await _testRequirementService.GetTestReqmtByIdAsync(testCode, buyer);

            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to retrieve test plan item.",
                    errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }

            TestPlanItem model = _mapper.Map<TestPlanItem>(result.Data);
            model.IsEdit = true;
            await PopulateTestCodeDropdownAsync(model);
            return PartialView("_AddEditTestPlan", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TestPlanItem item)
        {
            if (!ModelState.IsValid)
            {
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
            }

            TestRequirementDto dto = _mapper.Map<TestRequirementDto>(item);
            ApiResponseDto<TestRequirementDto> result = await _testRequirementService.UpdateTestReqmtAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = "Test plan item updated successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update test plan item.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD: DELETE ──────────────────────────────────────────────────────

        [HttpDelete]
        public async Task<IActionResult> Delete(string testCode, string buyer)
        {
            ApiResponseDto<bool> result = await _testRequirementService.DeleteTestReqmtAsync(testCode, buyer);

            if (result.Success)
            {
                return Json(new { success = true, message = "Test plan item deleted successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete test plan item.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── REC UNIT PRICE ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetRecUnitPrice(string testCode, string? projectBuyerCode = null)
        {
            if (string.IsNullOrWhiteSpace(testCode))
                return Json(new { success = false, recUnitPrice = 0 });

            ApiResponseDto<TestRequirementDto> result =
                await _testRequirementService.GetTestReqmtPricingAsync(testCode, projectBuyerCode);

            return result.Success && result.Data != null
                ? Json(new { success = true, recUnitPrice = result.Data.RecUnitPrice ?? 0 })
                : Json(new { success = false, recUnitPrice = 0 });
        }

        // ── TOTAL COST ────────────────────────────────────────────────────────
        public async Task<IActionResult> GetTotalTestCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job Code is required.", totalTestCost = 0 });
            }

            QueryParameters<string> query = new() { Page = 1, PageSize = 9999 };
            ApiResponseDto<List<TestRequirementDto>> result =
                await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, jobCode);

            if (result.Success && result.Data != null)
            {
                decimal total = result.Data.Sum(r => (r.UnitPrice ?? 0) * (decimal)(r.NoRequired ?? 0));
                return Json(new { success = true, totalTestCost = total });
            }

            return Json(new { success = false, message = "Failed to retrieve total test cost.", totalTestCost = 0 });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private static bool IsDuplicateError(ApiErrorDto error)
        {
            var code = error.Code ?? string.Empty;
            if (code.Equals("CONFLICT", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("DUPLICATE", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("BUSINESS_RULE_VIOLATION", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (error.Message ?? string.Empty).Contains("already exists", StringComparison.OrdinalIgnoreCase);
        }

        private async Task PopulateTestCodeDropdownAsync(TestPlanItem model)
        {
            ApiResponseDto<List<TestorProductDto>> response =
                await _testorProductService.GetAllTestorProductsAsync();

            model.TestCodeOptions = response.Data == null
                ? new List<SelectListItem>()
                : response.Data
                    .Select(t => new SelectListItem
                    {
                        Value = t.ItemCode,
                        Text  = $"{t.ItemCode}|{t.ItemDescription ?? string.Empty}|{t.UnitPriceVla?.ToString("F2") ?? "0.00"}",
                        Selected = string.Equals(model.TestCode, t.ItemCode, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
        }
    }
}
