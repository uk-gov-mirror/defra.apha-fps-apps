using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectProfitabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;

        public ProjectProfitabilityController(
            IMapper mapper,
            IProgramService programService,
            IProjectService projectService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _programService = programService;
            _projectService = projectService;
            _appStateService = appStateService;
        }

        public async Task<IActionResult> Index(string? programNo = null, string? source = null)
        {
            var isProjectGroupMode = string.Equals(source, "projectgroup", StringComparison.OrdinalIgnoreCase);

            var grid = GetProfitabilityGridConfig(isProjectGroupMode);

            var model = new ProjectProfitabilityViewModel
            {
                WorkTypeFilter = "all",
                ProfitabilityGrid = grid,
                IsProjectGroupMode = isProjectGroupMode
            };

            if (isProjectGroupMode)
            {
                var projectGroupList = await GetProjectGroupListAsync();
                var selectedProjectGroup = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProjectGroup);

                selectedProjectGroup = !string.IsNullOrWhiteSpace(selectedProjectGroup) && projectGroupList.Any(p => p.Value == selectedProjectGroup)
                    ? selectedProjectGroup
                    : projectGroupList.FirstOrDefault()?.Value ?? string.Empty;

                await _appStateService.SetSessionAsync(SessionKeys.SelectedProjectGroup, selectedProjectGroup);

                model.ProjectGroupList = projectGroupList;
                model.SelectedProjectGroup = selectedProjectGroup;
            }
            else
            {
                var programmeList = await GetProgrammeListAsync();

                if (string.IsNullOrWhiteSpace(programNo))
                    programNo = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProgramNo);

                var selectedProgramNo = !string.IsNullOrWhiteSpace(programNo) && programmeList.Any(p => p.Value == programNo)
                    ? programNo!
                    : programmeList.FirstOrDefault()?.Value ?? string.Empty;

                await _appStateService.SetSessionAsync(SessionKeys.SelectedProgramNo, selectedProgramNo);

                model.ProgrammeList = programmeList;
                model.SelectedProgramNo = selectedProgramNo;
            }

            return View(model);
        }

        /// <summary>
        /// Loads the Project Profitability DataGrid partial.
        /// Called by the _DataGrid gridManager via jQuery POST.
        /// Extra params (programNo, workTypeFilter) are merged in by getProjectProfitabilityExtraFilters().
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectProfitabilityGrid(
            PaginationFilter<string> request,
            string? programNo = null,
            string? projectGroup = null,
            string? workTypeFilter = "all")
        {
            var isProjectGroupMode = !string.IsNullOrWhiteSpace(projectGroup);

            // When no programme/project group is selected, return an empty grid so the page shows no data.
            if (!isProjectGroupMode && string.IsNullOrWhiteSpace(programNo))
                return PartialView("_DataGrid", BuildEmptyProfitabilityGridConfig(request, isProjectGroupMode));

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;            

            var query = _mapper.Map<QueryParameters<string>>(request);

            var response = isProjectGroupMode
                ? await _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup!, workTypeFilter ?? "all")
                : await _projectService.GetProjectProfitabilityAsync(query, programNo!, workTypeFilter ?? "all");

            if (!response.Success)
                return StatusCode(500, response.Errors);

            var items = _mapper.Map<List<ProjectProfitabilityItem>>(
                response.Data ?? new List<ProjectProfitabilityDto>());

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ProjectProfitabilityItem>
            {
                GridId = "isProjectProfitGrid",
                Title = isProjectGroupMode ? "Project Group Profitability" : "Project Profitability",
                KeyProperty = "JobCode",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectJobcodeTotal",
                ExtraFilterMethod = "getProjectProfitabilityExtraFilters",
                BindGridUrl = "/FPS/ProjectProfitability/LoadProjectProfitabilityGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectProfitabilityItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns programme-level summary figures (target + surplus/shortfall).
        /// Called separately after the grid reloads to avoid a double render.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitabilitySummary(
            [FromQuery] string? programNo,
            [FromQuery] string? projectGroup,
            [FromQuery] string workTypeFilter = "all")
        {
            var isProjectGroupMode = !string.IsNullOrWhiteSpace(projectGroup);

            if (!isProjectGroupMode && string.IsNullOrWhiteSpace(programNo))
                return Ok(new { programmeTarget = (decimal?)null, programmeSurplusShortfall = 0m });

            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };

            var response = isProjectGroupMode
                ? await _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup!, workTypeFilter)
                : await _projectService.GetProjectProfitabilityAsync(query, programNo!, workTypeFilter);

            if (!response.Success)
                return StatusCode(500, response.Errors);

            var items = response.Data ?? new List<ProjectProfitabilityDto>();
            var programmeTarget = items.FirstOrDefault()?.ProgrammeTarget;
            var sumProfit = items.Sum(i => i.JcProfit);
            return Ok(new
            {
                programmeTarget,
                programmeSurplusShortfall = sumProfit - (programmeTarget ?? 0m),
                totalStaffCosts      = items.Sum(i => i.JcTotalStaffCosts),
                totalTestCosts       = items.Sum(i => i.JcTotalTestCosts),
                totalAnimalCosts     = items.Sum(i => i.JcTotalAnimalCosts),
                totalAdditionalCosts = items.Sum(i => i.JcTotalAdditionalCosts),
                totalCosts           = items.Sum(i => i.TotalCosts),
                totalBudget          = items.Sum(i => i.BudgetCvl ?? 0m),
                totalProfit          = sumProfit,
                totalTargetProfit    = items.Sum(i => i.TargetProfit),
                totalOffTarget       = items.Sum(i => i.OffTarget)
            });
        }

        private DataGridConfig<ProjectProfitabilityItem> GetProfitabilityGridConfig(bool isProjectGroupMode = false)
        {            return new DataGridConfig<ProjectProfitabilityItem>
            {
                GridId = "isProjectProfitGrid",
                Title = isProjectGroupMode ? "Project Group Profitability" : "Project Profitability",
                KeyProperty = "JobCode",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectJobcodeTotal",
                ExtraFilterMethod = "getProjectProfitabilityExtraFilters",
                BindGridUrl = "/FPS/ProjectProfitability/LoadProjectProfitabilityGrid",
                Data = new List<ProjectProfitabilityItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ProjectProfitabilityItem>(),
                Pagination = new PaginationModel()
            };
        }

        private DataGridConfig<ProjectProfitabilityItem> BuildEmptyProfitabilityGridConfig(
            PaginationFilter<string> request, bool isProjectGroupMode)
        {
            var paginationModel = new PaginationModel
            {
                SortColumn = request.SortBy,
                SortDirection = request.Descending
            };

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            return new DataGridConfig<ProjectProfitabilityItem>
            {
                GridId = "isProjectProfitGrid",
                Title = isProjectGroupMode ? "Project Group Profitability" : "Project Profitability",
                KeyProperty = "JobCode",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectJobcodeTotal",
                ExtraFilterMethod = "getProjectProfitabilityExtraFilters",
                BindGridUrl = "/FPS/ProjectProfitability/LoadProjectProfitabilityGrid",
                Data = new List<ProjectProfitabilityItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ProjectProfitabilityItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task<List<SelectListItem>> GetProgrammeListAsync()
        {
            var response = await _programService.GetAllProgramsAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .OrderBy(p => p.ProgramNo)
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramNo,
                    Text = string.IsNullOrWhiteSpace(p.ProgramName)
                        ? p.ProgramNo
                        : $"{p.ProgramNo} — {p.ProgramName}"
                })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetProjectGroupListAsync()
        {
            var response = await _projectService.GetProjectGroupsByUserAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .OrderBy(g => g.ProjectGroupName)
                .Where(g => !string.IsNullOrWhiteSpace(g.ProjectGroupName))
                .Select(g => new SelectListItem
                {
                    Value = g.ProjectGroupName,
                    Text = g.ProjectGroupName
                })
                .ToList();
        }
    }
}
