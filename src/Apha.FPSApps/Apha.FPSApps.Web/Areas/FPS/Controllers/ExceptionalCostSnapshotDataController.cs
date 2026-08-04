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
    public class ExceptionalCostSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;

        public ExceptionalCostSnapshotDataController(IMapper mapper, IProjectService projectService)
        {
            _mapper = mapper;
            _projectService = projectService;
        }

        /// <summary>
        /// GET /FPS/ExceptionalCostSnapshotData — renders the snapshot exceptional costs page with an empty grid.
        /// The grid fetches its first page via the LoadExceptionalCostDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new ExceptionalCostSnapshotViewModel
            {
                ExceptionalCostSnapshotGrid = new DataGridConfig<ExceptionalCostSnapshotItem>
                {
                    GridId = "exceptionalCostSnapshotGrid",
                    Title = "Snapshot Exceptional Costs",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/ExceptionalCostSnapshotData/LoadExceptionalCostDataGrid",
                    Data = new List<ExceptionalCostSnapshotItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<ExceptionalCostSnapshotItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/ExceptionalCostSnapshotData/LoadExceptionalCostDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadExceptionalCostDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetExceptionalCostDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ExceptionalCostSnapshotItem>> GetExceptionalCostDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _projectService.GetProjectExceptionalCostsPagedAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ExceptionalCostSnapshotItem>>(response.Data.ToList())
                : new List<ExceptionalCostSnapshotItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ExceptionalCostSnapshotItem>
            {
                GridId = "exceptionalCostSnapshotGrid",
                Title = "Snapshot Exceptional Costs",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/ExceptionalCostSnapshotData/LoadExceptionalCostDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ExceptionalCostSnapshotItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
