using Apha.FPSApps.Application.Interfaces.FPS;
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
    public class MiscReportsController : Controller
    {
        private readonly IProfitCentreService _profitCentreService;
        private readonly ITestsRequiredByWgService _testsRequiredByWgService;
        private readonly ITestsRequiredByRcService _testsRequiredByRcService;

        public MiscReportsController(
            IProfitCentreService profitCentreService,
            ITestsRequiredByWgService testsRequiredByWgService,
            ITestsRequiredByRcService testsRequiredByRcService)
        {
            _profitCentreService = profitCentreService;
            _testsRequiredByWgService = testsRequiredByWgService;
            _testsRequiredByRcService = testsRequiredByRcService;
        }

        /// <summary>
        /// Displays the Misc Reports page. The grid stays empty until a Resource Centre is
        /// selected, mirroring the Budget Bids Query page behaviour.
        /// </summary>
        public async Task<IActionResult> Index(string? report = null, string? profitCentre = null)
        {
            var profitCentreOptions = await GetProfitCentreSelectListAsync();
            var year = GetSelectedFpsYear();

            var grid = await BuildGridAsync(profitCentre, report);

            return View(new MiscReportsViewModel
            {
                Grid = grid,
                ProfitCentreOptions = profitCentreOptions,
                SelectedProfitCentre = profitCentre,
                SelectedReport = report,
                FpsYear = year
            });
        }

        /// <summary>
        /// Reloads the Misc Reports grid partial for the selected Resource Centre.
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadGrid(string? profitCentre, string? report = null, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var grid = await BuildGridAsync(profitCentre, report, sortBy, descending, filter, page, pageSize);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<Dictionary<string, string?>>> BuildGridAsync(string? profitCentre, string? report, string? sortBy = null, bool descending = false, string? filter = null, int page = 1, int pageSize = 20)
        {
            var rows = new List<Dictionary<string, string?>>();
            var isRcReport = string.Equals(report, "TestManagerRcPivot", StringComparison.OrdinalIgnoreCase);

            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "ProfitCentre",    DisplayName = "Resource Centre", ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 160 }
            };

            if (!isRcReport)
            {
                columns.Add(new() { PropertyName = "WorkGroup", DisplayName = "Work Group", ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 160 });
            }

            columns.Add(new() { PropertyName = "TestCode",        DisplayName = "Test Code",       ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 120 });
            columns.Add(new() { PropertyName = "ItemDescription", DisplayName = "Item Description", ColumnType = GridColumnType.ReadOnly, IsFilterable = true, Width = 260, CssClass = "grid-column-wrap" });
            columns.Add(new() { PropertyName = "ProjectedTotal",  DisplayName = "Projected Total", ColumnType = GridColumnType.ReadOnly, IsFilterable = false, Width = 120 });
            columns.Add(new() { PropertyName = "UnitPrice",       DisplayName = "Unit Price",      ColumnType = GridColumnType.RoundTwoDecimal, IsFilterable = false, Width = 120 });

            // Only populate the grid once a report has been selected from the side navigation.
            if (!string.IsNullOrWhiteSpace(report))
            {
                if (isRcReport)
                {
                    var rcResponse = await _testsRequiredByRcService.GetTestsRequiredByRcAsync(profitCentre);
                    if (rcResponse.Success && rcResponse.Data != null)
                    {
                        foreach (var item in rcResponse.Data)
                        {
                            rows.Add(new Dictionary<string, string?>
                            {
                                ["ProfitCentre"]    = item.ProfitCentre,
                                ["TestCode"]        = item.TestCode,
                                ["ItemDescription"] = item.ItemDescription,
                                ["ProjectedTotal"]  = item.ProjectedTotal?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                ["UnitPrice"]       = item.UnitPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            });
                        }
                    }
                }
                else
                {
                    var response = await _testsRequiredByWgService.GetTestsRequiredByWgAsync(profitCentre);
                    if (response.Success && response.Data != null)
                    {
                        foreach (var item in response.Data)
                        {
                            rows.Add(new Dictionary<string, string?>
                            {
                                ["ProfitCentre"]    = item.ProfitCentre,
                                ["WorkGroup"]       = item.WorkGroup,
                                ["TestCode"]        = item.TestCode,
                                ["ItemDescription"] = item.ItemDescription,
                                ["ProjectedTotal"]  = item.ProjectedTotal?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                ["UnitPrice"]       = item.UnitPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            });
                        }
                    }
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
                GridId            = "miscReportsGrid",
                KeyProperty       = "TestCode",
                AllowAdd          = false,
                AllowEdit         = false,
                AllowDelete       = false,
                ShowPagination    = true,
                ExtraFilterMethod = "getMiscReportsExtraFilters",
                BindGridUrl       = "/FPS/MiscReports/LoadGrid",
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

        private static List<Dictionary<string, string?>> ApplySorting(List<Dictionary<string, string?>> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy) || rows.Count == 0)
                return rows;

            Func<Dictionary<string, string?>, string?> cellSelector =
                r => r.TryGetValue(sortBy, out var v) ? v : null;

            // When every populated cell in the sorted column is numeric, order it as a
            // number rather than text so values like 100 sort after 9 (not before).
            if (IsNumericColumn(rows, cellSelector))
            {
                Func<Dictionary<string, string?>, decimal?> numericSelector =
                    r => decimal.TryParse(cellSelector(r), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out var d)
                        ? d
                        : (decimal?)null;

                return descending
                    ? rows.OrderByDescending(numericSelector).ToList()
                    : rows.OrderBy(numericSelector).ToList();
            }

            return descending
                ? rows.OrderByDescending(cellSelector, StringComparer.OrdinalIgnoreCase).ToList()
                : rows.OrderBy(cellSelector, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool IsNumericColumn(
            List<Dictionary<string, string?>> rows,
            Func<Dictionary<string, string?>, string?> cellSelector)
        {
            var hasValue = false;
            foreach (var row in rows)
            {
                var value = cellSelector(row);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                hasValue = true;
                if (!decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    return false;
                }
            }

            return hasValue;
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
