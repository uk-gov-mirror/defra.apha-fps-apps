using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    public class TestSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestorProductService _testorProductService;

        public TestSnapshotDataController(IMapper mapper, ITestorProductService testorProductService)
        {
            _mapper = mapper;
            _testorProductService = testorProductService;
        }

        /// <summary>
        /// GET /FPS/TestSnapshotData — renders the snapshot tests page with an empty grid.
        /// The grid fetches its first page via the LoadTestSnapshotDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new TestSnapshotDataViewModel
            {
                SnapShotTestDataGrid = new DataGridConfig<TestSnapshotItem>
                {
                    GridId = "snapShotTestDataGrid",
                    Title = "Snapshot Tests",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/TestSnapshotData/LoadTestSnapshotDataGrid",
                    Data = new List<TestSnapshotItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<TestSnapshotItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/TestSnapshotData/LoadTestSnapshotDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadTestSnapshotDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetTestSnapshotDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestSnapshotItem>> GetTestSnapshotDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _testorProductService.GetTestSnapshotPagedAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestSnapshotItem>>(response.Data.ToList())
                : new List<TestSnapshotItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestSnapshotItem>
            {
                GridId = "snapShotTestDataGrid",
                Title = "Snapshot Tests",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/TestSnapshotData/LoadTestSnapshotDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestSnapshotItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
