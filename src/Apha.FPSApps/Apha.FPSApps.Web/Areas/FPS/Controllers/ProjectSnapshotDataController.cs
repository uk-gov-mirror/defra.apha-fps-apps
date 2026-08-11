using Apha.FPSApps.Application.Dtos.FPS;
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
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public ProjectSnapshotDataController(IMapper mapper, IProjectService projectService)
        {
            _mapper = mapper;
            _projectService = projectService;
        }

        /// <summary>
        /// GET /FPS/ProjectSnapshotData — renders the snapshot projects page with an empty grid.
        /// The grid fetches its first page via the LoadProjectSnapshotDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new ProjectSnapshotDataViewModel
            {
                SnapShotProjectDataGrid = new DataGridConfig<ProjectSnapshotItem>
                {
                    GridId = "snapShotProjectDataGrid",
                    Title = "Snapshot Projects",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/ProjectSnapshotData/LoadProjectSnapshotDataGrid",
                    Data = new List<ProjectSnapshotItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<ProjectSnapshotItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/ProjectSnapshotData/LoadProjectSnapshotDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectSnapshotDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetProjectSnapshotDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectSnapshotItem>> GetProjectSnapshotDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _projectService.GetPagedProjectSnapshotDataAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProjectSnapshotItem>>(response.Data.ToList())
                : new List<ProjectSnapshotItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectSnapshotItem>
            {
                GridId = "snapShotProjectDataGrid",
                Title = "Snapshot Projects",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/ProjectSnapshotData/LoadProjectSnapshotDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectSnapshotItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
