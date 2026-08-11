using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class StaffEffortController : Controller
    {
        private readonly ICostBookProjectSummaryService _projectSummaryService;
        private readonly ICostBookYearlyDetailsService _yearlyDetailsService;
        private readonly IMapper _mapper;

        public StaffEffortController(
            ICostBookProjectSummaryService projectSummaryService,
            ICostBookYearlyDetailsService yearlyDetailsService,
            IMapper mapper)
        {
            _projectSummaryService = projectSummaryService;
            _yearlyDetailsService = yearlyDetailsService;
            _mapper = mapper;
        }

        
        public async Task<IActionResult> Index(string projectId)
        {
            var headerResponse = await _yearlyDetailsService.GetProjectHeaderAsync(projectId);
            if (!headerResponse.Success || headerResponse.Data is null)
                return RedirectToAction("Index", "Projects");

            var grid = await BuildGridAsync(projectId);

            return View(new StaffEffortViewModel
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

        
        private async Task<DataGridConfig<StaffEffortPivotRow>> BuildGridAsync(
            string projectId, PaginationFilter<string>? request = null)
        {
            var query = request != null
                ? _mapper.Map<QueryParameters<string>>(request)
                : new QueryParameters<string> { Page = 1, PageSize = 10 };

            var response = await _projectSummaryService.GetStaffEffortAsync(projectId, query);
            var pivot = response.Success && response.Data != null
                ? response.Data
                : new StaffEffortPivotDto();

            var filterDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(request?.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            
            var rows = pivot.Rows
                .Where(r => !string.IsNullOrWhiteSpace(r.WorkGroup))               
                .Select(r =>
                {
                    var row = new StaffEffortPivotRow
                    {
                        Project   = r.Project,
                        WorkGroup = r.WorkGroup,
                        GradeCode = r.GradeCode,
                        Name      = r.Name,
                        Total     = Fmt(r.Total)   
                    };

                    for (int i = 0; i < pivot.Years.Count && i < 10; i++)
                    {
                        int year = pivot.Years[i];
                        decimal? value = r.YearlyAmounts.TryGetValue(year, out double v)
                            ? Fmt(v)               
                            : null;
                        switch (i)
                        {
                            case 0: row.Y1  = value; break;
                            case 1: row.Y2  = value; break;
                            case 2: row.Y3  = value; break;
                            case 3: row.Y4  = value; break;
                            case 4: row.Y5  = value; break;
                            case 5: row.Y6  = value; break;
                            case 6: row.Y7  = value; break;
                            case 7: row.Y8  = value; break;
                            case 8: row.Y9  = value; break;
                            case 9: row.Y10 = value; break;
                        }
                    }
                    return row;
                }).ToList();

            if (!string.IsNullOrWhiteSpace(query.SortBy)
                && query.SortBy.StartsWith("Y", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(query.SortBy[1..], out var yearIndex)
                && yearIndex >= 1 && yearIndex <= 10)
            {
                rows = query.Descending
                    ? rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenByDescending(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Name)
                          .ToList()
                    : rows.OrderBy(r => GetYearValue(r, yearIndex).HasValue ? 0 : 1)
                          .ThenBy(r => GetYearValue(r, yearIndex))
                          .ThenBy(r => r.Name)
                          .ToList();
            }

            var columns = new List<DataGridColumn>
            {
                new() { PropertyName = "Project",   DisplayName = "Project",    ColumnType = GridColumnType.Text, IsFilterable = false, Width = 100 },
                new() { PropertyName = "WorkGroup", DisplayName = "Work Group", ColumnType = GridColumnType.Text, IsFilterable = true,  Width = 120 },
                new() { PropertyName = "GradeCode", DisplayName = "Grade",      ColumnType = GridColumnType.Text, IsFilterable = true,  Width = 80  },
                new() { PropertyName = "Name",      DisplayName = "Name",       ColumnType = GridColumnType.Text, IsFilterable = true,  Width = 150 },
                new() { PropertyName = "Total",     DisplayName = "Total",      ColumnType = GridColumnType.Text, IsFilterable = false, Width = 90  }
                //                                                              
            };

            for (int i = 0; i < pivot.Years.Count && i < 10; i++)
            {
                columns.Add(new DataGridColumn
                {
                    PropertyName = $"Y{i + 1}",
                    DisplayName  = pivot.Years[i].ToString(),
                    ColumnType   = GridColumnType.Text,  
                    IsFilterable = false,
                    Width        = 90
                });
            }

            return new DataGridConfig<StaffEffortPivotRow>
            {
                GridId         = "staffEffortGrid",
                KeyProperty    = "Name",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = $"/CostBook/StaffEffort/LoadGrid?projectId={Uri.EscapeDataString(projectId)}",
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

        
        /// <summary>Rounds to 1dp and strips trailing decimal zeros so ToString() renders like MS Access (e.g. 15 not 15.0).</summary>
        private static decimal Fmt(double v)
            => decimal.Parse(Math.Round((decimal)v, 1, MidpointRounding.AwayFromZero).ToString("G29"));

        private static decimal? GetYearValue(StaffEffortPivotRow row, int yearIndex)
            => yearIndex switch
            {
                1 => row.Y1,
                2 => row.Y2,
                3 => row.Y3,
                4 => row.Y4,
                5 => row.Y5,
                6 => row.Y6,
                7 => row.Y7,
                8 => row.Y8,
                9 => row.Y9,
                10 => row.Y10,
                _ => null
            };
    }
}
