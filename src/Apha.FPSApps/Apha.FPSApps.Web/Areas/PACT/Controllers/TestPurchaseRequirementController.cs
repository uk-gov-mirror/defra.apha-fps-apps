using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
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

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class TestPurchaseRequirementController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testReqmtService;
        private readonly ITestorProductService _testorProductService;
        private readonly IProjectService _projectService;

        public TestPurchaseRequirementController(
            IMapper mapper,
            ITestRequirementService testReqmtService,
            ITestorProductService testorProductService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _testReqmtService = testReqmtService;
            _testorProductService = testorProductService;
            _projectService = projectService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? parentProject)
        {
            var defaultRequest = new PaginationFilter<string>();
            var grid = await BuildTestPurchaseRequiremntGridAsync(defaultRequest, parentProject ?? string.Empty);

            var origin = TempData.Peek("PactOrigin") as string;
            var viewModel = new TestPurchaseRequirementViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                NavigationOrigin = origin ?? "Project",
                TestPurchaseReqGrid = grid,
            };

            return View(viewModel);
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestPurchaseReqGrid(
            PaginationFilter<string> request, string testCode)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestPurchaseRequiremntGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetTestPurchaseReq(
            string? testCode, string? buyer, string? parentProject)
        {
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.BuyerOptions = await GetBuyerSelectListAsync();

            if (string.IsNullOrWhiteSpace(testCode))
            {
                var model = new TestPurchaseRequirementItem
                {
                    TestCode = string.Empty,
                    Buyer = buyer ?? string.Empty,
                    ProjectBuyerCode = parentProject,
                    Active = 1
                };               

                return PartialView("_AddEditTestPurchaseRequirement", model);
            }

            var result = await _testReqmtService.GetTestReqmtByIdAsync(testCode!, buyer ?? string.Empty);
            if (!result.Success || result.Data == null) return NotFound();
            var editModel = _mapper.Map<TestPurchaseRequirementItem>(result.Data);
            editModel.IsEdit = true;
            return PartialView("_AddEditTestPurchaseRequirement", editModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTestPurchaseReq([FromBody] TestPurchaseRequirementItem model)
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

            var dto = _mapper.Map<TestRequirementDto>(model);
            var isEdit = model.IsEdit;

            ApiResponseDto<TestRequirementDto> result;
            string successMsg;

            if (isEdit)
            {
                result = await _testReqmtService.UpdateTestReqmtAsync(dto);
                successMsg = "Test Purchase Requirement updated successfully.";
            }
            else
            {
                result = await _testReqmtService.CreateTestReqmtAsync(dto);
                successMsg = "Test Purchase Requirement saved successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to save Test Purchase Requirement.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestPurchaseReq(string testCode, string buyer)
        {
            var result = await _testReqmtService.DeleteTestReqmtAsync(testCode, buyer);
            return result.Success
                ? Json(new { success = true, message = "Test Purchase Requirement deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Purchase Requirement.",
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
                recUnitPrice = result.Data.RecUnitPrice,
                isDefraProject = string.IsNullOrWhiteSpace(projectCode) ? (short?)null : result.Data.IsDefraProject
            });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<TestPurchaseRequirementItem>> BuildTestPurchaseRequiremntGridAsync(
            PaginationFilter<string> request, string parentProject)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testReqmtService.GetPagedTestReqmtbyProjectAsync(query, parentProject);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestPurchaseRequirementItem>>(response.Data)
                : new List<TestPurchaseRequirementItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestPurchaseRequirementItem>
            {
                GridId = "testPurchaseReqGrid",
                Title = "Tests to Buy....",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Buyer",
                AddFunction = "addTestPurchaseReq",
                EditFunction = "editTestPurchaseReq",
                DeleteFunction = "deleteTestPurchaseReq",
                ExtraFilterMethod = "getTestPurchaseReqExtraFilters",
                BindGridUrl = "/PACT/TestPurchaseRequirement/LoadTestPurchaseReqGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestPurchaseRequirementItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(t => new SelectListItem(t.ItemCode, t.ItemCode))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetBuyerSelectListAsync()
        {
            var response = await _projectService.GetAllPactProjectsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(p => new SelectListItem(p.ParentProject, p.ParentProject))
                    .ToList()
                : new List<SelectListItem>();
        }
    }
}