using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class MonthlyOutputLogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _logService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ITestorProductService _TestorProductService;
        private readonly IProjectService _projectService;

        public MonthlyOutputLogController(
            IMapper mapper,
            IPactMonthlyOutputService logService,
            IWorkGroupService workGroupService,
            ITestorProductService TestorProductService,
            IProjectService projectService)
        {
            _mapper = mapper;
            _logService = logService;
            _workGroupService = workGroupService;
            _TestorProductService = TestorProductService;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index()
        {
            var workGroupsResponse = await _workGroupService.GetAllWorkGroupsAsync();
            var testsResponse = await _TestorProductService.GetAllTestorProductsAsync();
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var grid = await BuildLogGrid(defaultRequest,null,null,null,null,null,null,null,null);

            var viewModel = new MonthlyOutputLogViewModel
            {
                LogGrid = grid,
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                TestCodeOptions = testsResponse.Success && testsResponse.Data != null
                    ? testsResponse.Data
                        .Select(t => new SelectListItem(t.ItemCode, t.ItemCode))
                        .DistinctBy(x => x.Value)
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                ProjectOptions = projectsResponse.Success && projectsResponse.Data != null
                    ? projectsResponse.Data
                        .Select(p => new SelectListItem(
                            $"{p.ParentProject} — {p.ProjectTitle}", p.ParentProject))
                        .OrderBy(x => x.Value)
                        .ToList()
                    : new List<SelectListItem>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            PaginationFilter<string> request,
            string? workGroup,
            string? testCode,
            string? buyer,
            string? buyingTest,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            // If no search criteria provided, return empty grid (similar to initial page load)
            if (!HasSearchCriteria(workGroup, testCode, buyer, buyingTest, dateImported, month, userId, insertDelete))
            {
                var emptyGridConfig = await BuildLogGrid(request, null, null, null, null, null, null, null, null);
                return PartialView("_DataGrid", emptyGridConfig);
            }

            var gridConfig = await BuildLogGrid(request, workGroup, testCode, buyer, buyingTest,
                dateImported, month, userId, insertDelete);

            return PartialView("_DataGrid", gridConfig);
        }

        private static bool HasSearchCriteria(
            string? workGroup,
            string? testCode,
            string? buyer,
            string? buyingTest,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete) =>
                !string.IsNullOrWhiteSpace(workGroup) ||
                !string.IsNullOrWhiteSpace(testCode) ||
                !string.IsNullOrWhiteSpace(buyer) ||
                !string.IsNullOrWhiteSpace(buyingTest) ||
                dateImported.HasValue ||
                month.HasValue ||
                !string.IsNullOrWhiteSpace(userId) ||
                !string.IsNullOrWhiteSpace(insertDelete);

        private async Task<DataGridConfig<MonthlyOutputLogItem>> BuildLogGrid(
           PaginationFilter<string> request,
           string? workGroup,
           string? testCode,
           string? buyer,
           string? buyingTest,
           DateTime? dateImported,
           double? month,
           string? userId,
           string? insertDelete)
        {
            List<MonthlyOutputLogItem> items = [];
            PaginationModel pagination;

            var effectiveBuyer = !string.IsNullOrWhiteSpace(buyingTest) ? buyingTest : buyer;

            var filter = new MonthlyOutputLogFilterDto
            {
                WorkGroup = workGroup,
                TestCode = testCode,
                Buyer = effectiveBuyer,
                DateImported = dateImported,
                Month = month,
                UserId = userId,
                InsertDelete = insertDelete
            };

            if (HasSearchCriteria(workGroup, testCode, buyer, buyingTest, dateImported, month, userId, insertDelete))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _logService.SearchAsync(query, filter);
                items = response.Data != null ? _mapper.Map<List<MonthlyOutputLogItem>>(response.Data) : [];
                pagination = response.Pagination != null
                        ? _mapper.Map<PaginationModel>(response.Pagination)
                        : new PaginationModel();
            }
            else
            {
                pagination = new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var gridConfig = new DataGridConfig<MonthlyOutputLogItem>
            {
                GridId = "moLogGrid",
                Title = "Monthly Output Log",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyOutputLogItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict,
                ExtraFilterMethod = "getExtraFilters_moLogGrid",
                BindGridUrl = "/PACT/MonthlyOutputLog/Search"
            };

            return gridConfig;
        }
    }
}
