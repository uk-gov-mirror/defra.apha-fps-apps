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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class TestPlanCrossTabController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestPlanCrossTabService _testPlanCrossTabService;

        public TestPlanCrossTabController(IMapper mapper, ITestPlanCrossTabService testPlanCrossTabService)
        {
            _mapper = mapper;
            _testPlanCrossTabService = testPlanCrossTabService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await BuildViewModelAsync(new PaginationFilter<string> { PageSize = 20 });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = await BuildViewModelAsync(request);
            return PartialView("_DataGrid", model.Grid);
        }

        private async Task<TestPlanCrossTabViewModel> BuildViewModelAsync(PaginationFilter<string> request)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testPlanCrossTabService.GetPagedTestPlanCrossTabAsync(query);

            var filterDict = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                try { filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? []; }
                catch { /* ignore malformed filter */ }
            }

            var pagination = new PaginationModel
            {
                PageNumber    = request.Page > 0 ? request.Page : 1,
                PageSize      = request.PageSize > 0 ? request.PageSize : 20,
                SortColumn    = request.SortBy,
                SortDirection = request.Descending
            };

            var columns = new List<DataGridColumn>();
            var rows    = new List<Dictionary<string, string?>>();

            if (response.Success && response.Data is not null)
            {
                pagination.TotalRecords = response.Data.TotalCount;
                pagination.PageNumber   = response.Data.Page;
                pagination.PageSize     = response.Data.PageSize;

                rows = response.Data.Rows;

                columns = response.Data.Columns.Select(col => new DataGridColumn
                {
                    PropertyName = col,
                    DisplayName  = GetColumnDisplayName(col),
                    IsVisible    = true,
                    IsEditable   = false,
                    IsFilterable = col.Equals("testcode",         StringComparison.OrdinalIgnoreCase)
                                || col.Equals("shortdescription", StringComparison.OrdinalIgnoreCase),
                    ColumnType   = GridColumnType.RoundTwoDecimal,
                    Width        = 100
                }).ToList();
            }

            var grid = new DataGridConfig<Dictionary<string, string?>>
            {
                GridId         = "testPlanCrossTabGrid",
                Columns        = columns,
                Data           = rows,
                ShowPagination = true,
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                AllowCopy      = false,
                AllowView      = false,
                AllowExport    = false,
                KeyProperty    = "testcode",
                BindGridUrl    = "/FPS/TestPlanCrossTab/LoadGrid",
                Pagination     = pagination,
                CurrentFilters = filterDict
            };

            return new TestPlanCrossTabViewModel { Grid = grid };
        }

        private static string GetColumnDisplayName(string col) => col switch
        {
            "plan_total"    => "Plan Total",
            "req_totalcost" => "PC Total Cost",
            _ when col.StartsWith("pc_", StringComparison.OrdinalIgnoreCase) => col[3..],
            _               => col
        };
    }
}