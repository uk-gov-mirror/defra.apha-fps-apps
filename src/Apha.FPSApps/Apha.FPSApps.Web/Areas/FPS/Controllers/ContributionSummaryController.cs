using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ContributionSummaryController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IContributionSummaryService _service;
        private readonly IProfitCentreService _profitCentreService;

        public ContributionSummaryController(
            IMapper mapper,
            IContributionSummaryService service,
            IProfitCentreService profitCentreService)
        {
            _mapper              = mapper;
            _service             = service;
            _profitCentreService = profitCentreService;
        }

        // ?? Page load ????????????????????????????????????????????????????????

        /// <summary>Renders the page with the Selling PC dropdown only; grid loads via AJAX.</summary>
        public async Task<IActionResult> Index()
        {
            var vm = new ContributionSummaryViewModel
            {
                SellingProfitCentres = await GetProfitCentreSelectListAsync()
            };
            return View(vm);
        }

        // ?? Grid load (AJAX, called by DataGrid on every page/sort/filter change) ??

        /// <summary>
        /// Returns the paginated row grid partial for the selected Selling PC.
        /// Accepts the standard <see cref="PaginationFilter{T}"/> posted by the DataGrid component.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadData(PaginationFilter<string> request, string sellingPc)
        {
            if (IsSellingPcMissing(sellingPc, out var badRequest))
                return badRequest!;

            var gridConfig = await BuildRowGridAsync(request, sellingPc);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns the totals partial for the selected Selling PC.
        /// Called once after the grid first loads.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoadTotals(string sellingPc)
        {
            if (IsSellingPcMissing(sellingPc, out var badRequest))
                return badRequest!;

            var totalsResult = await _service.GetTotalsAsync(sellingPc);
            var totals = totalsResult.Success ? totalsResult.Data : null;
            return PartialView("_ContributionSummaryTotals", totals);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private bool IsSellingPcMissing(string sellingPc, out IActionResult? result)
        {
            if (string.IsNullOrWhiteSpace(sellingPc))
            {
                result = BadRequest("Selling PC is required.");
                return true;
            }
            result = null;
            return false;
        }

        private async Task<DataGridConfig<ContributionSummaryRowItem>> BuildRowGridAsync(
            PaginationFilter<string> request, string sellingPc)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? [];

            // Fetch all rows for this PC then apply client-side pagination
            var rowsResult = await _service.GetRowsAsync(sellingPc);
            var allRows    = rowsResult.Success && rowsResult.Data != null
                             ? rowsResult.Data
                             : [];

            var items = _mapper.Map<List<ContributionSummaryRowItem>>(allRows)
                               .DistinctBy(r => (r.WgGrade, r.WorkGroup, r.ProfitCentreGrade))
                               .ToList();

            // Apply column filters if supplied
            if (filterDict.TryGetValue("WgGrade", out var wgFilter) && !string.IsNullOrWhiteSpace(wgFilter))
                items = items.Where(r => r.WgGrade != null && r.WgGrade.Contains(wgFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (filterDict.TryGetValue("WorkGroup", out var wgNameFilter) && !string.IsNullOrWhiteSpace(wgNameFilter))
                items = items.Where(r => r.WorkGroup != null && r.WorkGroup.Contains(wgNameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            if (filterDict.TryGetValue("ProfitCentreGrade", out var pcgFilter) && !string.IsNullOrWhiteSpace(pcgFilter))
                items = items.Where(r => r.ProfitCentreGrade != null && r.ProfitCentreGrade.Contains(pcgFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            // Sort
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                items = request.SortBy switch
                {
                    "WgGrade"           => request.Descending ? items.OrderByDescending(r => r.WgGrade).ToList()           : items.OrderBy(r => r.WgGrade).ToList(),
                    "WorkGroup"         => request.Descending ? items.OrderByDescending(r => r.WorkGroup).ToList()         : items.OrderBy(r => r.WorkGroup).ToList(),
                    "ProfitCentreGrade" => request.Descending ? items.OrderByDescending(r => r.ProfitCentreGrade).ToList() : items.OrderBy(r => r.ProfitCentreGrade).ToList(),
                    "Hrs"               => request.Descending ? items.OrderByDescending(r => r.Hrs).ToList()               : items.OrderBy(r => r.Hrs).ToList(),
                    "Fec"               => request.Descending ? items.OrderByDescending(r => r.Fec).ToList()               : items.OrderBy(r => r.Fec).ToList(),
                    "Contribution"      => request.Descending ? items.OrderByDescending(r => r.Contribution).ToList()      : items.OrderBy(r => r.Contribution).ToList(),
                    _                   => items
                };
            }

            var totalRecords = items.Count;
            var pageSize     = request.PageSize > 0 ? request.PageSize : 10;
            var pageNumber   = request.Page   > 0 ? request.Page   : 1;
            var paged        = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new DataGridConfig<ContributionSummaryRowItem>
            {
                GridId            = "contributionSummaryGrid",
                Title             = $"Resource Center: {sellingPc}",
                KeyProperty       = "WgGrade",
                ShowCheckboxColumn = false,
                ShowPagination    = true,
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                AllowExport       = false,
                ExtraFilterMethod = "getContributionSummaryExtraFilters",
                BindGridUrl       = $"/FPS/ContributionSummary/LoadData?sellingPc={Uri.EscapeDataString(sellingPc)}",
                Data              = paged,
                Columns           = GridDataProvider.GetColumnsDefination<ContributionSummaryRowItem>(),
                // Column group header row matching frmTimeSellerPC:
                //   5 ungrouped | 3 Total Planned Time | 3 Assured Planned Time | 2 Rate Efficacy Checker
                ColumnGroups      =
                [
                    new() { Label = "",                        Span = 4 },
                    new() { Label = "Total Planned Time",      Span = 3 },
                    new() { Label = "Assured Planned Time",    Span = 3 },
                    new() { Label = "Rate \"Efficacy\" Checker", Span = 2 },
                ],
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = pageSize,
                    SortColumn    = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters    = filterDict
            };
        }

        private async Task<List<SelectListItem>> GetProfitCentreSelectListAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem { Value = p.ProfitCentreId, Text = $"{p.ProfitCentreId} - {p.ProfitCentreName}" })
                    .ToList();
            }
            return [];
        }
    }
}