using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.StateManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectAnimalPlanActualController : Controller
    {
        private readonly IAnimalPlanService _animalPlanService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;

        public ProjectAnimalPlanActualController(
            IAnimalPlanService animalPlanService,
            IProjectService projectService,
            IAppStateService appStateService)
        {
            _animalPlanService = animalPlanService;
            _projectService = projectService;
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

            var animalPlanGrid = new DataGridConfig<AnimalPlanItem>
            {
                GridId = "animalPlanGrid",
                Title = "Planned Animals (FPS)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "IndCounter",
                AddFunction = "addAnimalPlan",
                EditFunction = "editAnimalPlan",
                DeleteFunction = "deleteAnimalPlan",
                ExtraFilterMethod = "getAnimalPlanExtraFilters",
                BindGridUrl = $"/FPS/AnimalJob/LoadAnimalPlanGrid?title={Uri.EscapeDataString("Planned Animals (FPS)")}",
                Data = new List<AnimalPlanItem>(),
                Columns = GridDataProvider.GetColumnsDefination<AnimalPlanItem>(),
                Pagination = new PaginationModel()
            };

            var actualAnimalCostGrid = new DataGridConfig<ActualProjectCostItem>
            {
                GridId = "actualAnimalCostGrid",
                Title = "Actual Animal Costs (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "SubContCounter",
                ExtraFilterMethod = "getActualAnimalExtraFilters",
                BindGridUrl = "/FPS/ActualAnimalCost/LoadActualAnimalCostGrid",
                Data = new List<ActualProjectCostItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ActualProjectCostItem>(),
                Pagination = new PaginationModel()
            };

            decimal totalPlannedCost = selectedProjectCode != string.Empty
                ? (await _animalPlanService.GetTotalAnimalCostAsync(selectedProjectCode)).Data
                : 0m;

            var model = new ProjectAnimalPlanActualViewModel
            {
                SelectedProjectCode = selectedProjectCode,
                ProjectTitle = projectInfo?.ProjectTitle ?? string.Empty,
                Program = projectInfo?.Program ?? string.Empty,
                Contract = projectInfo?.Contract ?? string.Empty,
                TotalPlannedCost = totalPlannedCost,
                TotalActualCost = 0m,
                PercentOfPlan = 0.0,
                ProjectList = projectList,
                AnimalPlanGrid = animalPlanGrid,
                ActualAnimalCostGrid = actualAnimalCostGrid
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalPlannedCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
                return Json(new { success = false, message = "Job code is required.", totalPlannedCost   = 0 });

            ApiResponseDto<decimal> result = await _animalPlanService.GetTotalAnimalCostAsync(jobCode);
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
