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
    public class TestCapabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IProjectService _projectService;
        private readonly ITestorProductService _testorProductService;

        public TestCapabilityController(
            IMapper mapper,
            ITestCapabilityService testCapabilityService,
            IProjectService projectService,
            ITestorProductService testorProductService)
        {
            _mapper = mapper;
            _testCapabilityService = testCapabilityService;
            _projectService = projectService;
            _testorProductService = testorProductService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var portfolioOptions = await GetPortfolioSelectListAsync();
            var workGroupOptions = await GetWorkGroupSelectListAsync();

            var viewModel = new TestCapabilityViewModel
            {
                PortfolioOptions = portfolioOptions,
                WorkGroupOptions = workGroupOptions,
                TestCapabilityGrid = new DataGridConfig<TestCapabilityItem>
                {
                    GridId = "testCapabilityGrid",
                    Title = "Portfolio Components",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowRowSelection = false,
                    KeyProperty = "TestCode",
                    AddFunction = "addTestCapability",
                    EditFunction = "editTestCapability",
                    DeleteFunction = "deleteTestCapability",
                    ExtraFilterMethod = "getTestCapabilityExtraFilters",
                    BindGridUrl = "/FPS/TestCapability/LoadTestCapabilityGrid",
                    Data = new List<TestCapabilityItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                    Pagination = new PaginationModel(),
                    AllowAdd = true,
                    AllowEdit = true,
                    AllowDelete = true
                }
            };

            return View(viewModel);
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestCapabilityGrid(
            PaginationFilter<string> request, string? portfolio = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestCapabilityGridAsync(request, portfolio);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── TEST CAPABILITY CRUD ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestCapability(string? portfolio)
        {
            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.TestorProductUnitCosts = await GetTestorProductUnitCostsAsync();
            var model = new TestCapabilityItem { PlanPortfolio = portfolio ?? string.Empty };
            return PartialView("_AddEditTestCapability", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestCapability([FromBody] TestCapabilityItem model)
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

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _testCapabilityService.CreateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Portfolio Component created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Portfolio Component.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestCapability(string testCode, string workGroup)
        {
            var result = await _testCapabilityService.GetTestCapabilityByIdAsync(testCode, workGroup);
            if (!result.Success)
                return NotFound($"Portfolio Component with TestCode '{testCode}' and WorkGroup '{workGroup}' not found.");

            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();

            // Fetch product options once and reuse for both the ViewBag and the description lookup.
            // ItemDescription is not stored on the TestCapability entity, so it must be resolved
            // from the TestorProduct list (which uses "Code – Description" text format).
            var testorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.TestorProductOptions = testorProductOptions;
            ViewBag.TestorProductUnitCosts = await GetTestorProductUnitCostsAsync();

            var item = _mapper.Map<TestCapabilityItem>(result.Data);

            var matchingOption = testorProductOptions.FirstOrDefault(o => o.Value == testCode);
            if (matchingOption is not null)
            {
                var sepIdx = matchingOption.Text.IndexOf(" \u2013 ");
                item.ItemDescription = sepIdx >= 0
                    ? matchingOption.Text[(sepIdx + 3)..]
                    : matchingOption.Text;
            }

            return PartialView("_AddEditTestCapability", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestCapability([FromBody] TestCapabilityItem model)
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

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _testCapabilityService.UpdateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Portfolio Component updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Portfolio Component.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestCapability(string testCode, string workGroup)
        {
            var result = await _testCapabilityService.DeleteTestCapabilityAsync(testCode, workGroup);
            return result.Success
                ? Json(new { success = true, message = "Portfolio Component deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Portfolio Component.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<TestCapabilityItem>> BuildTestCapabilityGridAsync(
            PaginationFilter<string> request, string? portfolio)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(query, portfolio);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestCapabilityItem>>(response.Data)
                : new List<TestCapabilityItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestCapabilityItem>
            {
                GridId = "testCapabilityGrid",
                Title = "Portfolio Components",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = false,
                KeyProperty = "TestCode",
                AddFunction = "addTestCapability",
                EditFunction = "editTestCapability",
                DeleteFunction = "deleteTestCapability",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",
                BindGridUrl = "/FPS/TestCapability/LoadTestCapabilityGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true
            };
        }

        private async Task<List<SelectListItem>> GetPortfolioSelectListAsync()
        {
            var response = await _projectService.GetAllProjectsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem(
                        string.IsNullOrWhiteSpace(p.ProjectTitle)
                            ? p.ParentProject
                            : $"{p.ParentProject} \u2013 {p.ProjectTitle}",
                        p.ParentProject))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetWorkGroupSelectListAsync()
        {
            var response = await _testCapabilityService.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(t => new SelectListItem(
                        string.IsNullOrWhiteSpace(t.ItemDescription) ? t.ItemCode : $"{t.ItemCode} \u2013 {t.ItemDescription}",
                        t.ItemCode))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<Dictionary<string, decimal?>> GetTestorProductUnitCostsAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .GroupBy(t => t.ItemCode)
                    .ToDictionary(g => g.Key, g => g.First().UnitPriceVla)
                : new Dictionary<string, decimal?>();
        }
    }
}
