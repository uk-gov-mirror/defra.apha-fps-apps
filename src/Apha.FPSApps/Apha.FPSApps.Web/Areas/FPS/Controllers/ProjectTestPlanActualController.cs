using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.StateManagement;
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
    public class ProjectTestPlanActualController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMonthlyOutputService _projTestPlanActualService;
        private readonly IProjectService _projectService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly IAppStateService _appStateService;

        public ProjectTestPlanActualController(
            IMapper mapper,
            IMonthlyOutputService projTestPlanActualService,
            IProjectService projectService,
            ITestRequirementService testRequirementService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _projTestPlanActualService = projTestPlanActualService;
            _projectService = projectService;
            _testRequirementService = testRequirementService;
            _appStateService = appStateService;
        }

        public async Task<IActionResult> Index(string? projectCode = null)
        {
            var projectList = await GetProjectListAsync();

            if (string.IsNullOrWhiteSpace(projectCode))
                projectCode = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProjectCode);

            var selectedProjectCode = !string.IsNullOrWhiteSpace(projectCode)
                && projectList.Any(p => p.Value == projectCode)
                ? projectCode
                : projectList.FirstOrDefault()?.Value ?? string.Empty;

            var projectInfo = await GetProjectInfoAsync(selectedProjectCode);

            var testPlanGrid = new DataGridConfig<TestPlanActualItem>
            {
                GridId = "testPlanGrid",
                Title = "Planned Time (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteTestPlanJob",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadTestPlanGrid",
                Data = new List<TestPlanActualItem>(),
                Columns = GridDataProvider.GetColumnsDefination<TestPlanActualItem>(),
                Pagination = new PaginationModel()
            };

            var compareTests2Grid = new DataGridConfig<ActualTestOutputItem>
            {
                GridId = "compareTests2Grid",
                Title = "Actual Tests (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareTests2",
                ExtraFilterMethod = "getCompareTests2ExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadCompareTests2Grid",
                Data = new List<ActualTestOutputItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ActualTestOutputItem>(),
                Pagination = new PaginationModel()
            };

            var totalPlannedCost = selectedProjectCode != string.Empty
                ? await ComputeTotalPlannedCostAsync(selectedProjectCode)
                : 0m;

            var model = new ProjectTestPlanActualViewModel
            {
                SelectedProjectCode = selectedProjectCode,
                ProjectTitle = projectInfo?.ProjectTitle ?? string.Empty,
                Program = projectInfo?.Program ?? string.Empty,
                Contract = projectInfo?.Contract ?? string.Empty,
                TotalPlannedCost = totalPlannedCost,
                ProjectList = projectList,
                TestPlanGrid = testPlanGrid,
                CompareTests2Grid = compareTests2Grid
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestPlanGrid(PaginationFilter<string> request, string? jobCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, jobCode ?? string.Empty);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestPlanActualItem>>(response.Data)
                : new List<TestPlanActualItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var grid = new DataGridConfig<TestPlanActualItem>
            {
                GridId = "testPlanGrid",
                Title = "Planned Time (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "TestCode",
                DeleteFunction = "deleteTestPlan",
                ExtraFilterMethod = "getTestPlanExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadTestPlanGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestPlanActualItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadCompareTests2Grid(PaginationFilter<string> request, string? projectCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new Dictionary<string, string>();
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var priceLookup = await GetPriceLookupAsync(projectCode ?? string.Empty);
            var pagedData = await _projTestPlanActualService.GetMonthlyOutputByProjectAsync(queryParameters, projectCode ?? string.Empty, priceLookup);

            var items = pagedData.Data != null ? _mapper.Map<List<ActualTestOutputItem>>(pagedData.Data) : new List<ActualTestOutputItem>();
            var paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ActualTestOutputItem>
            {
                GridId = "compareTests2Grid",
                Title = "Actual Tests (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareTests2",
                ExtraFilterMethod = "getCompareTests2ExtraFilters",
                BindGridUrl = "/FPS/ProjectTestPlanActual/LoadCompareTests2Grid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ActualTestOutputItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectInfo(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projectService.GetProjectByIdAsync(projectCode);
            if (result.Success && result.Data != null)
                return Json(new { success = true, projectTitle = result.Data.ProjectTitle, program = result.Data.Program, contract = result.Data.Contract });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Project not found.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalPlannedCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var totalPlannedCost = await ComputeTotalPlannedCostAsync(projectCode);
            return Json(new { success = true, totalPlannedCost });
        }

        private async Task<decimal> ComputeTotalPlannedCostAsync(string projectCode)
        {
            var allQuery = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var result = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(allQuery, projectCode);
            if (!result.Success || result.Data == null) return 0m;
            return result.Data.Sum(t => (t.UnitPrice ?? 0m) * (decimal)(t.NoRequired ?? 0.0));
        }

        private async Task<Dictionary<(string, string), decimal>> GetPriceLookupAsync(string projectCode)
        {
            var allQuery = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var result = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(allQuery, projectCode);
            return result.Data?
                .ToDictionary(t => (t.TestCode ?? string.Empty, t.Buyer ?? string.Empty), t => t.UnitPrice ?? 0m)
                ?? new Dictionary<(string, string), decimal>();
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalActualCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var priceLookup = await GetPriceLookupAsync(projectCode);
            var result = await _projTestPlanActualService.GetTotalActualByProjectAsync(projectCode, priceLookup);
            if (result.Success)
                return Json(new { success = true, totalCost = result.Data });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve actual totals.", totalCost = 0, errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMonthlyOutput(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
                return Json(new { success = false, message = "Row key is required." });

            var parts = rowKey.Split('|');
            if (parts.Length != 4)
                return Json(new { success = false, message = "Invalid row key format." });

            var testCode  = parts[0];
            var buyer     = parts[1];
            var month     = double.TryParse(parts[2], out var m) ? m : 0;
            var workGroup = parts[3];

            var result = await _projTestPlanActualService.DeleteMonthlyOutputAsync(buyer, testCode, month, workGroup);
            if (result.Success)
                return Json(new { success = true, message = "Record deleted successfully" });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.", errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." }) });
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectLookup()
        {
            var response = await _projectService.GetAllProjectsAsync();
            if (!response.Success || response.Data == null)
                return Json(new List<object>());
            var data = response.Data
                .Select(p => new { parentProject = p.ParentProject, projectTitle = p.ProjectTitle ?? string.Empty })
                .ToList();
            return Json(data);
        }

        private async Task<List<SelectListItem>> GetProjectListAsync()
        {
            var result = await _projectService.GetAllProjectsAsync();
            if (result.Success && result.Data != null)
                return result.Data.Select(p => new SelectListItem { Value = p.ParentProject, Text = p.ParentProject }).ToList();
            return new List<SelectListItem>();
        }

        private async Task<ProjectDto?> GetProjectInfoAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode)) return null;
            var result = await _projectService.GetProjectByIdAsync(projectCode);
            return result.Success ? result.Data : null;
        }
    }
}