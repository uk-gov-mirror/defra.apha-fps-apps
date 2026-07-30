using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class RecreateSummaryLogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IRecreateSummaryService _logService;

        /// <summary>
        /// Initialises a new instance of <see cref="RecreateSummaryLogController"/>.
        /// </summary>
        /// <param name="mapper">AutoMapper instance used to map pagination filters and row collections to their corresponding view-model types.</param>
        /// <param name="logService">Application service that retrieves recreate summaries log data from the PACT API.</param>
        public RecreateSummaryLogController(IMapper mapper, IRecreateSummaryService logService)
        {
            _mapper = mapper;
            _logService = logService;
        }

        /// <summary>
        /// Renders the Recreate Summaries Log page.
        /// Fetches the full recreate summaries log dataset, builds the initial data-grid
        /// configuration with no sort applied, and returns the view.
        /// </summary>
        /// <returns>
        /// A <see cref="ViewResult"/> containing a <see cref="RecreateSummaryLogViewModel"/>
        /// with the populated grid.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            return View(new RecreateSummaryLogViewModel
            {
                LogsGrid = await BuildLogGrid(new PaginationFilter<string> { Filter = "{}" })
            });
        }

        /// <summary>
        /// Handles partial-page grid refreshes triggered by the client-side data-grid component
        /// (<c>_DataGrid.cshtml</c>) whenever the user pages, sorts, or filters the grid.
        /// Maps the incoming pagination/filter/sort request to query parameters, fetches an updated
        /// page of recreate summaries log data, and returns only the <c>_DataGrid</c> partial
        /// view so the page can be updated in-place without a full reload.
        /// </summary>
        /// <param name="request">Pagination, sort, and column-filter parameters submitted by the grid via AJAX POST.</param>
        /// <returns>
        /// A <see cref="PartialViewResult"/> rendering the <c>_DataGrid</c> partial with an updated
        /// <see cref="DataGridConfig{RecreateSummaryLogItem}"/> model.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LoadRecreateSummariesLogGrid(PaginationFilter<string> request)
        {
            var grid = await BuildLogGrid(request);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<RecreateSummaryLogItem>> BuildLogGrid(PaginationFilter<string> request)
        {
            var grid = RecreateSummariesLogGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _logService.GetRecreateSummaryLogAsync(query);

            grid.Data = response.Data != null ? _mapper.Map<List<RecreateSummaryLogItem>>(response.Data.data) : [];

            grid.Pagination = response.Pagination != null
                       ? _mapper.Map<PaginationModel>(response.Pagination)
                       : new PaginationModel();
            
            grid.Pagination.SortColumn = request.SortBy;
            grid.Pagination.SortDirection = request.Descending;

            return grid;
        }

        /// <summary>
        /// Returns the static <see cref="DataGridConfig{RecreateSummaryLogItem}"/> skeleton shared by
        /// both <see cref="Index"/> and <see cref="LoadRecreateSummariesLogGrid"/>.
        /// The configuration defines the grid identity, bound AJAX URL, column definitions,
        /// and interaction flags; it intentionally contains no data or pagination state so
        /// callers can populate those fields independently after calling this method.
        /// </summary>
        /// <returns>A new <see cref="DataGridConfig{RecreateSummaryLogItem}"/> with static configuration applied.</returns>
        private static DataGridConfig<RecreateSummaryLogItem> RecreateSummariesLogGridConfig() => new()
        {
            GridId = "releaseLogsGrid",
            Title = string.Empty,
            BindGridUrl = "/PACT/RecreateSummaryLog/LoadRecreateSummariesLogGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            AllowExport = false,
            AllowRowSelection = false,
            ShowPagination = true,
            Columns = GridDataProvider.GetColumnsDefination<RecreateSummaryLogItem>()
        };
    }
}
