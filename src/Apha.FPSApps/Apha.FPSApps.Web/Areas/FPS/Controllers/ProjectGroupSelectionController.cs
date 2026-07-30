using Apha.Common.Utilities.StateManagement;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Application.Interfaces.FPS;
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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectGroupSelectionController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;

        public ProjectGroupSelectionController(
            IMapper mapper,
            IProjectService projectService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _appStateService = appStateService;
        }

        /// <summary>
        /// Displays the Project Group Selection - a read-only project selection interface filtered by project group.
        /// </summary>
        public async Task<IActionResult> Index(string? projectGroup = null, string? projectSearch = null)
        {
            var projectGroupList = await GetProjectGroupListAsync();

            // Only use projectGroup if it is explicitly provided and valid — never fall back to session or first item
            var isValidProjectGroup = !string.IsNullOrWhiteSpace(projectGroup)
                && projectGroupList.Any(p => p.Value == projectGroup);
            var selectedProjectGroup = isValidProjectGroup ? projectGroup! : string.Empty;

            // Save to session only when user has made an explicit selection
            if (isValidProjectGroup)
                await _appStateService.SetSessionAsync(SessionKeys.SelectedProjectGroup, selectedProjectGroup);

            var defaultRequest = new PaginationFilter<string>();
            var grid = await BuildProjectsGridAsync(defaultRequest, selectedProjectGroup, projectSearch);

            var model = new ProjectGroupSelectionViewModel
            {
                SelectedProjectGroup = selectedProjectGroup,
                ProjectSearch = projectSearch ?? string.Empty,
                ProjectGroupList = projectGroupList,
                ProjectsGrid = grid
            };

            return View(model);
        }

        /// <summary>
        /// Returns a lightweight list of all project codes + project group for the Project lookup dropdown.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjectLookup()
        {
            var response = await _projectService.GetProjectLookupAsync();
            if (!response.Success || response.Data == null)
                return Json(new List<object>());

            var data = response.Data
                .Select(p => new { parentProject = p.ParentProject, projectGroup = p.ProjectGroup ?? string.Empty })
                .ToList();

            return Json(data);
        }

        /// <summary>
        /// Saves the selected project group to session (called client-side via AJAX).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveProjectGroupSession([FromBody] string projectGroup)
        {
            await _appStateService.SetSessionAsync(SessionKeys.SelectedProjectGroup, projectGroup);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LoadProjectsGrid(PaginationFilter<string> request, string projectGroup, string? projectSearch = null)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(projectGroup))
                return BadRequest(ModelState);

            // projectSearch may arrive as a standalone param or inside request.Filter as JSON
            if (string.IsNullOrWhiteSpace(projectSearch) && !string.IsNullOrWhiteSpace(request.Filter))
            {
                var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter);
                filterDict?.TryGetValue("projectSearch", out projectSearch);
            }

            var gridConfig = await BuildProjectsGridAsync(request, projectGroup, projectSearch);

            return PartialView("_DataGrid", gridConfig);
        }

        #region Private Helpers

        private async Task<List<SelectListItem>> GetProjectGroupListAsync()
        {
            var response = await _projectService.GetProjectGroupsByUserAsync();

            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .OrderBy(p => p.ProjectGroupName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectGroupName,
                    Text = p.ProjectGroupName
                })
                .ToList();
        }

        private async Task<DataGridConfig<ProjectGroupSelectionProjectItem>> BuildProjectsGridAsync(
            PaginationFilter<string> request, string projectGroup, string? projectSearch = null)
        {
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // Pass projectSearch as a server-side filter so the API filters before paging
            if (!string.IsNullOrWhiteSpace(projectSearch))
                queryParameters.Filter = JsonConvert.SerializeObject(new { ParentProject = projectSearch });

            var response = !string.IsNullOrWhiteSpace(projectGroup)
                ? await _projectService.GetProjectsByProjectGroupAsync(queryParameters, projectGroup)
                : null;

            var items = response?.Data != null
                ? response.Data.Select(p => new ProjectGroupSelectionProjectItem
                  {
                      ProjectGroup = p.ProjectGroup ?? string.Empty,
                      ParentProject = p.ParentProject ?? string.Empty
                  }).ToList()
                : new List<ProjectGroupSelectionProjectItem>();

            var pagination = response?.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProjectGroupSelectionProjectItem>
            {
                GridId = "projectsGrid",
                Title = "Projects",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = false,
                AllowView = true,
                EditFunction = "editProject",
                ViewFunction = "planProject",
                ExtraFilterMethod = "getProjectsExtraFilters",
                BindGridUrl = $"/FPS/ProjectGroupSelection/LoadProjectsGrid?projectGroup={Uri.EscapeDataString(projectGroup)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectGroupSelectionProjectItem>(),
                Pagination = pagination
            };
        }

        #endregion
    }
}
