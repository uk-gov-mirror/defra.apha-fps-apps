using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Reflection;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class ProjectCostsController : Controller
    {
        private readonly ICostBookProjectSummaryService _projectSummaryService;
        private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
        private readonly IMapper _mapper;

        public ProjectCostsController(
            ICostBookProjectSummaryService projectSummaryService,
            ICostBookYearlyDetailsService yearlyDetailsService,
            IMapper mapper)
        {
            _projectSummaryService = projectSummaryService;
            _yearlyDetailsService  = yearlyDetailsService;
            _mapper                = mapper;
        }

        public async Task<IActionResult> Index(string projectId)
        {
            var headerResponse = await _yearlyDetailsService.GetProjectHeaderAsync(projectId);
            if (!headerResponse.Success || headerResponse.Data is null)
                return RedirectToAction("Index", "Projects");

            var grid = await BuildGridAsync(projectId);

            return View(new ProjectCostsViewModel
            {
                ProjectId        = projectId,
                ProjectHeaderDto = headerResponse.Data,
                Grid             = grid
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadGrid(string projectId, PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(projectId, request);
            return PartialView("_DataGrid", grid);
        }

        
        private async Task<DataGridConfig<ProjectCostsPivotRow>> BuildGridAsync(
            string projectId, PaginationFilter<string>? request = null)
        {
            var query = request != null
                ? _mapper.Map<QueryParameters<string>>(request)
                : new QueryParameters<string> { Page = 1, PageSize = 5 };

            var response = await _projectSummaryService.GetProjectCostsPivotAsync(projectId, query);
            var pivot = response.Success && response.Data != null
                ? response.Data
                : new ProjectCostsPivotDto();

            var filterDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(request?.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            // Cap the number of year columns/values to a sensible maximum (10)
            var yearCount = Math.Min(pivot.Years.Count, 10);

            var rows = pivot.Rows.Select(r =>
            {
                var row = ProjectCostsPivotRow.Create(yearCount);
                row.Project = r.Project;
                row.Category = r.Category;
                row.Total = Fmt(r.Total);

                for (int i = 0; i < yearCount; i++)
                {
                    int year = pivot.Years[i];
                    decimal? value = r.YearlyAmounts.TryGetValue(year, out double v)
                        ? Fmt(v)
                        : null;

                    row.SetYearValue(i + 1, value);
                }

                return row;
            }).ToList();

            if (!string.IsNullOrWhiteSpace(query.SortBy)
                && query.SortBy.StartsWith("Y", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(query.SortBy[1..], out var yearIndex)
                && yearIndex >= 1 && yearIndex <= yearCount)
            {
                rows = query.Descending
                    ? rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenByDescending(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Category)
                          .ToList()
                    : rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenBy(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Category)
                          .ToList();
            }

            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "Project",  DisplayName = "Project",  ColumnType = GridColumnType.Text,     IsFilterable = false, Width = 100 },
                new() { PropertyName = "Category", DisplayName = "Category", ColumnType = GridColumnType.Text,     IsFilterable = true,  Width = 130 },
                new() { PropertyName = "Total",    DisplayName = "Total",    ColumnType = GridColumnType.GbpValue, IsFilterable = false, Width = 100 }
            };

            for (int i = 0; i < yearCount; i++)
            {
                columns.Add(new DataGridColumn
                {
                    PropertyName = $"Y{i + 1}",
                    DisplayName  = pivot.Years[i].ToString(),
                    ColumnType   = GridColumnType.GbpValue,
                    IsFilterable = false,
                    Width        = 90
                });
            }

            return new DataGridConfig<ProjectCostsPivotRow>
            {
                GridId         = "projectCostsGrid",
                KeyProperty    = "Category",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = $"/CostBook/ProjectCosts/LoadGrid?projectId={Uri.EscapeDataString(projectId)}",
                Columns        = columns,
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = new PaginationModel
                {
                    TotalRecords  = pivot.TotalCount,
                    PageNumber    = query.Page,
                    PageSize      = query.PageSize,
                    SortColumn    = query.SortBy,
                    SortDirection = query.Descending
                }
            };
        }

        /// <summary>Rounds to 2dp and strips trailing zeros so ToString() renders like MS Access (e.g. 1500 not 1500.00).</summary>
        private static decimal Fmt(double v)
            => decimal.Parse(Math.Round((decimal)v, 2, MidpointRounding.AwayFromZero).ToString("G29"));

        private static decimal? GetYearValue(ProjectCostsPivotRow row, int yearIndex)
        {
            var property = row.GetType().GetProperty($"Y{yearIndex}", BindingFlags.Instance | BindingFlags.Public);
            return property?.GetValue(row) as decimal?;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string projectId)
        {
            var bytes = await _projectSummaryService.ExportProjectSummaryToExcelAsync(projectId);
            var fileName = $"ProjectSummary_{projectId}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
