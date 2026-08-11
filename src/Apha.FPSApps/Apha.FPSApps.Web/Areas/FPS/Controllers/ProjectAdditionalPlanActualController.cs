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
    public class ProjectAdditionalPlanActualController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly IProjectService _projectService;
        private readonly IProjectSubContractService _projectSubContractService;
        private readonly IAppStateService _appStateService;

        public ProjectAdditionalPlanActualController(
            IMapper mapper,
            IAdditionalCostService additionalCostService,
            IProjectService projectService,
            IProjectSubContractService projectSubContractService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _additionalCostService = additionalCostService;
            _projectService = projectService;
            _projectSubContractService = projectSubContractService;
            _appStateService = appStateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? projectCode = null)
        {
            List<SelectListItem> projectList = await GetProjectListAsync();

            if (string.IsNullOrWhiteSpace(projectCode))
                projectCode = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProjectCode);

            string selectedProjectCode = !string.IsNullOrWhiteSpace(projectCode)
                && projectList.Any(p => p.Value == projectCode)
                ? projectCode
                : projectList.FirstOrDefault()?.Value ?? string.Empty;

            ProjectDto? projectInfo = await GetProjectInfoAsync(selectedProjectCode);

            var additionalCostPlanGrid = new DataGridConfig<AdditionalCostItemViewModel>
            {
                GridId = "additionalCostPlanGrid",
                Title = "Planned Additional Costs (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "Description",
                AddFunction = "addAdditionalCost",
                EditFunction = "editAdditionalCost",
                DeleteFunction = "deleteAdditionalCost",
                ExtraFilterMethod = "getAdditionalCostPlanExtraFilters",
                BindGridUrl = $"/FPS/AdditionalCostJob/LoadAdditionalCostGrid?title={Uri.EscapeDataString("Planned Additional Costs (FPS)")}",
                Data = new List<AdditionalCostItemViewModel>(),
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostItemViewModel>(),
                Pagination = new PaginationModel()
            };

            var actualAdditionalCostGrid = new DataGridConfig<ActualProjectCostItem>
            {
                GridId = "actualAdditionalCostGrid",
                Title = "Actual Additional Costs (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = true,
                KeyProperty = "SubContCounter",
                DeleteFunction = "deleteActualAdditionalCost",
                ExtraFilterMethod = "getActualAdditionalCostExtraFilters",
                BindGridUrl = "/FPS/ProjectAdditionalPlanActual/LoadActualAdditionalCostGrid",
                Data = new List<ActualProjectCostItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ActualProjectCostItem>(),
                Pagination = new PaginationModel()
            };

            decimal totalPlannedCost = selectedProjectCode != string.Empty
                ? (await _additionalCostService.GetTotalItemCostAsync(selectedProjectCode)).Data
                : 0m;

            var model = new ProjectAdditionalPlanActualViewModel
            {
                SelectedProjectCode = selectedProjectCode,
                ProjectTitle = projectInfo?.ProjectTitle ?? string.Empty,
                Program = projectInfo?.Program ?? string.Empty,
                Contract = projectInfo?.Contract ?? string.Empty,
                TotalPlannedCost = totalPlannedCost,
                TotalActualCost = 0m,
                PercentOfPlan = 0.0,
                ProjectList = projectList,
                AdditionalCostPlanGrid = additionalCostPlanGrid,
                ActualAdditionalCostGrid = actualAdditionalCostGrid
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalPlannedCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
                return Json(new { success = false, message = "Job code is required.", totalPlannedCost = 0 });

            ApiResponseDto<decimal> result = await _additionalCostService.GetTotalItemCostAsync(jobCode);
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

        [HttpPost]
        public async Task<IActionResult> LoadActualAdditionalCostGrid(PaginationFilter<string> request, string? projectCode = null)
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

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            
            var pagedData = await _projectSubContractService.GetFpsProjectSubContractsAsync(queryParameters, projectCode, filterByAnimalAcctCodes: false);
            List<ActualProjectCostItem> items = pagedData.Data != null
                ? _mapper.Map<List<ActualProjectCostItem>>(pagedData.Data)
                : new List<ActualProjectCostItem>();

            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = new DataGridConfig<ActualProjectCostItem>
            {
                GridId = "actualAdditionalCostGrid",
                Title = "Actual Additional Costs (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,  // ✅ Delete button hidden
                KeyProperty = "SubContCounter",
                DeleteFunction = "deleteActualAdditionalCost",
                ExtraFilterMethod = "getActualAdditionalCostExtraFilters",
                BindGridUrl = "/FPS/ProjectAdditionalPlanActual/LoadActualAdditionalCostGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ActualProjectCostItem>(null),
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

            ApiResponseDto<ProjectDto> result = await _projectService.GetProjectByIdAsync(projectCode);
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
        public async Task<IActionResult> GetTotalActualCost(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required.", totalActualCost = 0 });

            ApiResponseDto<decimal> result = await _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false);
            if (result.Success)
                return Json(new { success = true, totalActualCost = result.Data });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Could not retrieve actual cost.",
                totalActualCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int subContCounter)
        {
            ApiResponseDto<bool> result = await _projectSubContractService.DeleteAsync(subContCounter);
            if (result.Success)
                return Json(new { success = true, message = "Additional cost deleted successfully." });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete additional cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
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
            ApiResponseDto<List<ProjectDto>> result = await _projectService.GetAllProjectsAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem { Value = p.ParentProject, Text = p.ParentProject })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private async Task<ProjectDto?> GetProjectInfoAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return null;

            ApiResponseDto<ProjectDto> result = await _projectService.GetProjectByIdAsync(projectCode);
            return result.Success ? result.Data : null;
        }
    }
}
