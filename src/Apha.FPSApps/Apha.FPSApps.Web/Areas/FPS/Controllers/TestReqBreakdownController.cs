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
    public class TestReqBreakdownController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestReqBreakdownService _testReqBreakdownService;

        public TestReqBreakdownController(IMapper mapper, ITestReqBreakdownService testReqBreakdownService)
        {
            _mapper = mapper;
            _testReqBreakdownService = testReqBreakdownService;
        }

        public async Task<IActionResult> Index()
        {
            var grid = await BuildGridAsync(new PaginationFilter<string>());
            return View(new TestReqBreakdownViewModel { Grid = grid });
        }

        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<TestReqBreakdownItem>> BuildGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testReqBreakdownService.GetPlannedTestsByWorkgroupAsync(query);

            var rows = new List<TestReqBreakdownItem>();
            PaginationModel pagination = new();

            if (response.Success && response.Data != null)
            {
                rows = _mapper.Map<List<TestReqBreakdownItem>>(response.Data);

                if (response.Pagination != null)
                {
                    pagination.PageNumber = response.Pagination.PageNumber;
                    pagination.PageSize = response.Pagination.PageSize;
                    pagination.TotalRecords = response.Pagination.TotalRecords;
                }
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<TestReqBreakdownItem>
            {
                GridId = "testReqBreakdownGrid",
                KeyProperty = "TestCode",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                BindGridUrl = "/FPS/TestReqBreakdown/LoadGrid",
                Columns = GridDataProvider.GetColumnsDefination<TestReqBreakdownItem>(),
                Data = rows,
                CurrentFilters = filterDict,
                Pagination = pagination
            };
        }
    }
}
