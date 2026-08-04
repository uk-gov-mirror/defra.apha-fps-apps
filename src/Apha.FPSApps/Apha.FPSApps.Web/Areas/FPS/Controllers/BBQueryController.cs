using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class BBQueryController : Controller
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IProfitCentreService _profitCentreService;

        public BBQueryController(
            IWorkGroupService workGroupService,
            IBudgetBidsService budgetBidsService,
            IProfitCentreService profitCentreService)
        {
            _workGroupService = workGroupService;
            _budgetBidsService = budgetBidsService;
            _profitCentreService = profitCentreService;
        }

        /// <summary>
        /// Displays the Budget Bids cross-tab (BBQuery) page. The grid stays empty until a
        /// Resource Centre is selected. Data is built from the selected Resource Centre and the
        /// current FPS year, mirroring the Budget Bids cross-tab Excel report.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var year = GetSelectedFpsYear();

            var grid = await BuildGridAsync(null);

            return View(new BBQueryViewModel
            {
                Grid = grid,
                ProfitCentreOptions = profitCentreOptions,
                SelectedProfitCentre = null,
                FpsYear = year
            });
        }

        /// <summary>
        /// Reloads the BBQuery cross-tab grid partial for the selected Resource Centre.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadGrid(string? profitCentre, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var grid = await BuildGridAsync(profitCentre, sortBy, descending, filter, page, pageSize);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<Dictionary<string, string?>>> BuildGridAsync(string? profitCentre, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var rows = new List<Dictionary<string, string?>>();
            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "AccShortName", DisplayName = "AccShortName", ColumnType = GridColumnType.ReadOnly, IsFilterable = true,  Width = 160 },
                new() { PropertyName = "RowSummary",   DisplayName = "Row Summary",  ColumnType = GridColumnType.RoundTwoDecimal, IsFilterable = false, Width = 120 }
            };

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                // Reuse the same query flow as the Budget Bids cross-tab export:
                // Resource Centre -> workgroups -> bids -> [account][workgroup] = GenBid lookup.
                var wgResponse = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
                var workgroups = wgResponse.Success && wgResponse.Data != null
                    ? wgResponse.Data.Select(w => w.WorkGroupName).OrderBy(w => w).ToList()
                    : new List<string>();

                var allBids = new List<BidViewDto>();
                foreach (var wg in workgroups)
                {
                    var bidResponse = await _budgetBidsService.GetBidViewAsync(wg);
                    if (!bidResponse.Success || bidResponse.Data == null) continue;
                    allBids.AddRange(bidResponse.Data);
                }

                var bidLookup = allBids
                    .GroupBy(b => b.Account)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToDictionary(b => b.WorkGroupName, b => b.GenBid));

                var categoriesResponse = await _budgetBidsService.GetAccountCategoriesAsync();
                var accounts = categoriesResponse.Success && categoriesResponse.Data?.Count > 0
                    ? categoriesResponse.Data.Select(a => a.AccShortName).OrderBy(a => a).ToList()
                    : allBids.Select(b => b.Account).Distinct().OrderBy(a => a).ToList();

                foreach (var wg in workgroups)
                {
                    columns.Add(new DataGridColumn
                    {
                        PropertyName = wg,
                        DisplayName  = wg,
                        ColumnType   = GridColumnType.RoundTwoDecimal,
                        IsFilterable = false,
                        Width        = 110
                    });
                }

                foreach (var account in accounts)
                {
                    var row = new Dictionary<string, string?>
                    {
                        ["AccShortName"] = account
                    };
                    decimal rowTotal = 0;

                    foreach (var wg in workgroups)
                    {
                        decimal amount = 0;
                        if (bidLookup.TryGetValue(account, out var wgBids) &&
                            wgBids.TryGetValue(wg, out var value))
                        {
                            amount = value;
                        }

                        row[wg] = amount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        rowTotal += amount;
                    }

                    row["RowSummary"] = rowTotal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    rows.Add(row);
                }
            }

            var filters = ParseFilters(filter);
            rows = ApplyFilters(rows, filters);
            rows = ApplySorting(rows, sortBy, descending);

            var pageNumber = page > 0 ? page : 1;
            var itemsPerPage = pageSize > 0 ? pageSize : 20;
            var totalRecords = rows.Count;

            var pagedRows = rows
                .Skip((pageNumber - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new DataGridConfig<Dictionary<string, string?>>
            {
                GridId            = "bbQueryGrid",
                KeyProperty       = "AccShortName",
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                ShowPagination    = true,
                ExtraFilterMethod = "getBBQueryExtraFilters",
                BindGridUrl       = "/FPS/BBQuery/LoadGrid",
                Columns           = columns,
                Data              = pagedRows,
                CurrentFilters    = filters,
                Pagination        = new PaginationModel
                {
                    TotalRecords  = totalRecords,
                    PageNumber    = pageNumber,
                    PageSize      = itemsPerPage,
                    SortColumn    = sortBy,
                    SortDirection = descending
                }
            };
        }

        /// <summary>
        /// Parses the JSON filter payload posted by the DataGrid into a column/value map.
        /// Returns null when there is nothing to filter on.
        /// </summary>
        private static Dictionary<string, string>? ParseFilters(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;

            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(filter);
                if (parsed == null || parsed.Count == 0)
                    return null;

                var cleaned = parsed
                    .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return cleaned.Count > 0 ? cleaned : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Filters the cross-tab rows using a case-insensitive "contains" match on the fixed
        /// <c>AccShortName</c> column and a "contains" match on the string form of the
        /// <c>RowSummary</c> and dynamic workgroup columns.
        /// </summary>
        private static List<Dictionary<string, string?>> ApplyFilters(List<Dictionary<string, string?>> rows, Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0 || rows.Count == 0)
                return rows;

            return rows.Where(row => filters.All(f => RowMatchesFilter(row, f.Key, f.Value))).ToList();
        }

        private static bool RowMatchesFilter(Dictionary<string, string?> row, string column, string value)
        {
            var cellValue = row.TryGetValue(column, out var v) ? v : null;

            return cellValue != null &&
                   cellValue.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sorts the cross-tab rows by the requested column. Supports the fixed
        /// <c>AccShortName</c> and <c>RowSummary</c> columns as well as the dynamic
        /// workgroup columns whose values are held in the row dictionary.
        /// </summary>
        private static List<Dictionary<string, string?>> ApplySorting(List<Dictionary<string, string?>> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy) || rows.Count == 0)
                return rows;

            Func<Dictionary<string, string?>, object?> keySelector =
                r => r.TryGetValue(sortBy, out var v) ? v : null;

            var comparer = new BBQueryRowComparer(keySelector);

            return descending
                ? rows.OrderByDescending(r => r, comparer).ToList()
                : rows.OrderBy(r => r, comparer).ToList();
        }

        /// <summary>
        /// Compares cross-tab rows by an extracted key, ordering numeric values numerically
        /// and everything else as case-insensitive strings, with nulls sorted first.
        /// </summary>
        private sealed class BBQueryRowComparer : IComparer<Dictionary<string, string?>>
        {
            private readonly Func<Dictionary<string, string?>, object?> _keySelector;

            public BBQueryRowComparer(Func<Dictionary<string, string?>, object?> keySelector)
            {
                _keySelector = keySelector;
            }

            public int Compare(Dictionary<string, string?>? x, Dictionary<string, string?>? y)
            {
                var xKey = x is null ? null : _keySelector(x);
                var yKey = y is null ? null : _keySelector(y);

                if (xKey is null && yKey is null) return 0;
                if (xKey is null) return -1;
                if (yKey is null) return 1;

                if (TryToDecimal(xKey, out var xNum) && TryToDecimal(yKey, out var yNum))
                    return xNum.CompareTo(yNum);

                return string.Compare(xKey.ToString(), yKey.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            private static bool TryToDecimal(object value, out decimal result)
            {
                switch (value)
                {
                    case decimal d:
                        result = d;
                        return true;
                    case double db:
                        result = (decimal)db;
                        return true;
                    case int i:
                        result = i;
                        return true;
                    case long l:
                        result = l;
                        return true;
                    default:
                        return decimal.TryParse(value.ToString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out result);
                }
            }
        }

        private int GetSelectedFpsYear()
        {
            if (HttpContext.Items.TryGetValue("SelectedFPSYear", out var yearObj) &&
                yearObj != null &&
                int.TryParse(yearObj.ToString(), out var year))
            {
                return year;
            }

            return DateTime.Now.Year;
        }

        private async Task<List<SelectListItem>> GetProfitCentreSelectListAsync()
        {
            var response = await _profitCentreService.GetProfitCentresAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(pc => !string.IsNullOrWhiteSpace(pc.ProfitCentreId))
                .Select(pc => new SelectListItem(pc.ProfitCentreId, pc.ProfitCentreId))
                .ToList();
        }
    }
}
