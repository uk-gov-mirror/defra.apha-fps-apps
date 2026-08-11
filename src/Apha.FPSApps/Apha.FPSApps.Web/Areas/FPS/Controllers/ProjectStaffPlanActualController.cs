using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectStaffPlanActualController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITimeCostCalcsService _projPlanVsActualsStaffService;
        private readonly IProjectService _projectService;
        private readonly IStaffJobService _staffJobService;
        private readonly IAppStateService _appStateService;

        public ProjectStaffPlanActualController(
            IMapper mapper,
            ITimeCostCalcsService projPlanVsActualsStaffService,
            IProjectService projectService,
            IStaffJobService staffJobService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _projPlanVsActualsStaffService = projPlanVsActualsStaffService;
            _projectService = projectService;
            _staffJobService = staffJobService;
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

            var staffPlanGrid = new DataGridConfig<StaffJobItemViewModel>
            {
                GridId = "staffBookedGrid",
                Title = "Planned Time (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "StaffID",
                AddFunction = "addStaffJob",
                EditFunction = "editStaffJob",
                DeleteFunction = "deleteStaffJob",
                ExtraFilterMethod = "getStaffPlanExtraFilters",
                BindGridUrl = $"/FPS/StaffJob/LoadStaffJobGrid?title={Uri.EscapeDataString("Planned Time (FPS)")}",
                Data = new List<StaffJobItemViewModel>(),
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItemViewModel>(),
                Pagination = new PaginationModel()
            };

            var compareStaff2Grid = new DataGridConfig<CompareStaff2Item>
            {
                GridId = "compareStaff2Grid",
                Title = "Actual Time (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareStaff2",
                ExtraFilterMethod = "getCompareStaff2ExtraFilters",
                BindGridUrl = "/FPS/ProjectStaffPlanActual/LoadCompareStaff2Grid",
                Data = new List<CompareStaff2Item>(),
                Columns = GridDataProvider.GetColumnsDefination<CompareStaff2Item>(),
                Pagination = new PaginationModel()
            };

            var totalPlannedCost = selectedProjectCode != string.Empty
                ? (await _staffJobService.GetTotalStaffCostAsync(selectedProjectCode)).Data
                : 0m;

            var model = new ProjectStaffPlanActualViewModel
            {
                SelectedProjectCode = selectedProjectCode,
                ProjectTitle = projectInfo?.ProjectTitle ?? string.Empty,
                Program = projectInfo?.Program ?? string.Empty,
                Contract = projectInfo?.Contract ?? string.Empty,
                TotalPlannedCost = totalPlannedCost,
                ProjectList = projectList,
                StaffPlanGrid = staffPlanGrid,
                CompareStaff2Grid = compareStaff2Grid
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadCompareStaff2Grid(PaginationFilter<string> request, string? projectCode = null)
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
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _projPlanVsActualsStaffService.GetTimeCostCalcsByProjectAsync(queryParameters, projectCode ?? string.Empty);

            var items = new List<CompareStaff2Item>();
            if (pagedData.Data != null)
                items = _mapper.Map<List<CompareStaff2Item>>(pagedData.Data);

            var paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<CompareStaff2Item>
            {
                GridId = "compareStaff2Grid",
                Title = "Actual Time (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "RowKey",
                DeleteFunction = "deleteCompareStaff2",
                ExtraFilterMethod = "getCompareStaff2ExtraFilters",
                BindGridUrl = "/FPS/ProjectStaffPlanActual/LoadCompareStaff2Grid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<CompareStaff2Item>(null),
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
            {
                return Json(new
                {
                    success = true,
                    projectTitle = result.Data.ProjectTitle,
                    program = result.Data.Program,
                    contract = result.Data.Contract
                });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Project not found.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalPlannedCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _staffJobService.GetTotalStaffCostAsync(projectCode);
            if (result.Success)
                return Json(new { success = true, totalPlannedCost = result.Data });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve planned cost.",
                totalPlannedCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalActualCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projPlanVsActualsStaffService.GetTotalActualByProjectAsync(projectCode);
            if (result.Success && result.Data != null)
                return Json(new { success = true, totalHours = result.Data.TotalHours, totalCost = result.Data.TotalCost });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve actual totals.",
                totalHours = 0,
                totalCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTimeCostCalcs(string rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
                return Json(new { success = false, message = "Row key is required." });

            var parts = rowKey.Split('|');
            if (parts.Length != 5)
                return Json(new { success = false, message = "Invalid row key format." });

            var workgroup = parts[0];
            var jobCode   = parts[1];
            var project   = parts[2];
            var month     = double.TryParse(parts[3], out var m) ? m : 0;
            var staffId   = parts[4];

            var result = await _projPlanVsActualsStaffService.DeleteTimeCostCalcsAsync(workgroup, jobCode, project, month, staffId);
            if (result.Success)
                return Json(new { success = true, message = "Record deleted successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Returns a lightweight list of all projects (ParentProject + ProjectTitle) for the multi-column dropdown.
        /// </summary>
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
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject
                    })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private async Task<ProjectDto?> GetProjectInfoAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return null;

            var result = await _projectService.GetProjectByIdAsync(projectCode);
            return result.Success ? result.Data : null;
        }
    }
}
