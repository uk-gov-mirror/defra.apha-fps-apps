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
    public class BidSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IBudgetBidsService _budgetBidsService;

        public BidSnapshotDataController(IMapper mapper, IBudgetBidsService budgetBidsService)
        {
            _mapper = mapper;
            _budgetBidsService = budgetBidsService;
        }

        /// <summary>
        /// GET /FPS/BidSnapshotData — renders the snapshot bid page with an empty grid.
        /// The grid fetches its first page via the LoadSnapShotBidDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new SnapShotBidViewModel
            {
                SnapShotBidGrid = new DataGridConfig<GenericBidItem>
                {
                    GridId = "snapShotBidGrid",
                    Title = "Snapshot Bids",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/BidSnapshotData/LoadSnapShotBidDataGrid",
                    Data = new List<GenericBidItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<GenericBidItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/BidSnapshotData/LoadSnapShotBidDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadSnapShotBidDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetSnapShotBidDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<GenericBidItem>> GetSnapShotBidDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _budgetBidsService.GetGenericBidsPagedAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<GenericBidItem>>(response.Data.ToList())
                : new List<GenericBidItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<GenericBidItem>
            {
                GridId = "snapShotBidGrid",
                Title = "Snapshot Bids",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/BidSnapshotData/LoadSnapShotBidDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<GenericBidItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
