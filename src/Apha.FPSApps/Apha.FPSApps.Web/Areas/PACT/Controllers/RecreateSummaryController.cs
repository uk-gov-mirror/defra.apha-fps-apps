using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class RecreateSummaryController : Controller
    {
        private const string RecreateSummariesJobName = "RecreateSummary";

        private readonly IMapper _mapper;
        private readonly IRecreateSummaryService _service;
        private readonly IMonthService _monthService;
        private readonly ILogger<RecreateSummaryController> _logger;

        public RecreateSummaryController(
            IMapper mapper,
            IRecreateSummaryService service,
            IMonthService monthService,
            ILogger<RecreateSummaryController> logger)
        {
            _mapper = mapper;
            _service = service;
            _monthService = monthService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the Recreate Summaries page with the month dropdown, Create Summary button,
        /// and the job execution history grid.
        /// The button is disabled on page load when the job is already running.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var months = await GetMonthsListAsync();
            var canRun = await GetCanRunJobAsync();
            var grid = await BuildHistoryGridAsync(new PaginationFilter<string> { Filter = "{}" });

            return View(new RecreateSummariesViewModel
            {
                Months = months,
                CanRunJob = canRun,
                HistoryGrid = grid
            });
        }

        /// <summary>
        /// Handles partial grid refreshes for the batch job history.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadHistoryGrid(PaginationFilter<string> request)
        {
            var grid = await BuildHistoryGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        /// <summary>
        /// Triggers the RecreateSummaries batch job for the selected month.
        /// Returns JSON indicating success or failure so the page can react without a full reload.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> TriggerJob(int month)
        {
            var result = await _service.TriggerRecreateSummariesBatchJobAsync(month);

            if (result.Success)
            {
                _logger.LogInformation("Recreate Summary batch job triggered successfully with eventif -{a} and jobquueid -{b}.", result?.Data?.EventId, result?.Data?.Jobqueue.JobExecutionId);
                return Json(new { success = true });
            }

            // Return ALL errors so the client can display every message simultaneously
            var errors = result.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to trigger the job." }];

            return Json(new { success = false, errors });
        }

        private async Task<DataGridConfig<BatchJobHistoryItem>> BuildHistoryGridAsync(PaginationFilter<string> request)
        {
            var grid = HistoryGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _service.GetRecreateSummaryBatchJobHistoryAsync(query, RecreateSummariesJobName);

            grid.Data = response.Data != null
                ? _mapper.Map<List<BatchJobHistoryItem>>(response.Data)
                : new List<BatchJobHistoryItem>();

            grid.Pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();

            grid.Pagination.SortColumn = request.SortBy;
            grid.Pagination.SortDirection = request.Descending;

            return grid;
        }

        private static DataGridConfig<BatchJobHistoryItem> HistoryGridConfig() => new()
        {
            GridId = "summaryHistoryGrid",
            Title = string.Empty,
            BindGridUrl = "/PACT/RecreateSummary/LoadHistoryGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            Columns = GridDataProvider.GetColumnsDefination<BatchJobHistoryItem>()
        };

        private async Task<bool> GetCanRunJobAsync()
        {
            var result = await _service.CanRunRecreateSummaryBatchJobAsync(RecreateSummariesJobName);
            return result.Success && result.Data;
        }

        private async Task<List<SelectListItem>> GetMonthsListAsync()
        {
            var result = await _monthService.GetAllMonthsAsync();

            if (result?.Success == true && result.Data?.Count > 0)
            {
                return result.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Monthnumber.ToString(),
                        Text = $"{m.Monthnumber} - {m.Monthname}"
                    })
                    .ToList();
            }

            return [];
        }
    }
}
