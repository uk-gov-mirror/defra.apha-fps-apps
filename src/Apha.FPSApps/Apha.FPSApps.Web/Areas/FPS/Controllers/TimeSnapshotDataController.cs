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
    public class TimeSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;

        public TimeSnapshotDataController(IMapper mapper, IProgramService programService)
        {
            _mapper = mapper;
            _programService = programService;
        }

        /// <summary>
        /// GET /FPS/TimeSnapshotData — renders the snapshot time page with an empty grid.
        /// The grid fetches its first page via the LoadTimeSnapshotDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new TimeSnapshotDataViewModel
            {
                SnapShotTimeDataGrid = new DataGridConfig<TimeSnapshotItem>
                {
                    GridId = "snapShotTimeDataGrid",
                    Title = "Snapshot Time",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/TimeSnapshotData/LoadTimeSnapshotDataGrid",
                    Data = new List<TimeSnapshotItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<TimeSnapshotItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/TimeSnapshotData/LoadTimeSnapshotDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadTimeSnapshotDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetTimeSnapshotDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TimeSnapshotItem>> GetTimeSnapshotDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _programService.GetProgramTimeSnapshotAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TimeSnapshotItem>>(response.Data.ToList())
                : new List<TimeSnapshotItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TimeSnapshotItem>
            {
                GridId = "snapShotTimeDataGrid",
                Title = "Snapshot Time",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/TimeSnapshotData/LoadTimeSnapshotDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TimeSnapshotItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
