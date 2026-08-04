using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for the read-only Project Specific Query page.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectSpecificQueryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public ProjectSpecificQueryController(IMapper mapper, IProjectService projectService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        /// <summary>
        /// Displays the Project Specific Query page with the DataGrid.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetProjectSpecificQueryGridConfigAsync();

            var viewModel = new ProjectSpecificQueryViewModel
            {
                ProjectSpecificQueryGrid = gridConfig
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the Project Specific Query grid via AJAX for pagination, sorting and filtering.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectSpecificQueryGrid(PaginationFilter<string> request)
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

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var gridConfig = await GetProjectSpecificQueryGridConfigAsync(queryParameters, filterDict);

            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectSpecificQueryItem>> GetProjectSpecificQueryGridConfigAsync(QueryParameters<string>? query = null, Dictionary<string, string>? filterDict = null)
        {
            var response = await _projectService.GetPagedProjectSpecificQueryAsync(query ?? new QueryParameters<string>());

            var items = new List<ProjectSpecificQueryItem>();
            if (response.Success && response.Data != null)
                items = _mapper.Map<List<ProjectSpecificQueryItem>>(response.Data);

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            return new DataGridConfig<ProjectSpecificQueryItem>
            {
                GridId = "projectSpecificQueryGrid",
                Title = "Project Specifics Query",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "ParentProject",
                BindGridUrl = "/FPS/ProjectSpecificQuery/LoadProjectSpecificQueryGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectSpecificQueryItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
