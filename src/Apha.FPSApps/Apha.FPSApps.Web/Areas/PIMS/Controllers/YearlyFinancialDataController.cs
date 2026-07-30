using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Globalization;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class YearlyFinancialDataController : Controller
    {
        private readonly IMapper _mapper;

       
        private readonly IYearlyFinancialDataService _service;

        
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;

        public YearlyFinancialDataController(
            IMapper mapper,
            IYearlyFinancialDataService service,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _service = service;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
        }

        // ── Index ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? project, string? parentproject = null)
        {
            YearlyFinancialDataViewModel viewModel = new();

            
            await PopulateDropdownsAsync(viewModel);

            viewModel.SelectedProject = project ?? string.Empty;

            
            viewModel.Parentproject = parentproject ?? viewModel.SelectedProject;

            viewModel.HoursInDay = await GetRequiredDoubleSettingAsync("HoursInDay");
            viewModel.DaysInYear = await GetRequiredDoubleSettingAsync("DaysInYear");

            foreach (SelectListItem item in viewModel.ProjectList)
            {
                item.Selected = string.Equals(item.Value, viewModel.SelectedProject, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.SelectedProject))
            {
                ApiResponseDto<ProjectDetailDto> detailResult =
                    await _projectDetailsService.GetPimsDetailAsync(viewModel.SelectedProject);

                if (detailResult.Success && detailResult.Data != null)
                {
                    ProjectDetailDto detail = detailResult.Data;
                    viewModel.StartDate = detail.StartDate.HasValue
                        ? detail.StartDate.Value.ToString("dd/MM/yyyy")
                        : string.Empty;
                    viewModel.EndDate = detail.RevisedEndDate.HasValue
                        ? detail.RevisedEndDate.Value.ToString("dd/MM/yyyy")
                        : (detail.EndDate.HasValue
                            ? detail.EndDate.Value.ToString("dd/MM/yyyy")
                            : string.Empty);
                }
            }

           
            viewModel.CostCenterListGrid = new DataGridConfig<YearlyFinancialDataItem>
            {
                GridId             = "costCenterListGrid",
                Title              = "Yearly Financial Details",
                ShowCheckboxColumn = false,
                ShowPagination     = false,
                KeyProperty        = "Year",
                AllowAdd           = true,
                AddFunction        = "addYearlyFinancialData",
                AllowEdit          = true,
                EditFunction       = "editYearlyFinancialData",
                AllowDelete        = true,
                DeleteFunction     = "deleteYearlyFinancialData",
                AllowView          = false, 
                ViewFunction       = "viewYearlyFinancialData",
                ExtraFilterMethod  = "getYearlyFinancialDataExtraFilters",
                BindGridUrl        = "/PIMS/YearlyFinancialData/LoadYearlyFinancialDataGrid",
                Data               = [], 
                Columns            = GridDataProvider.GetColumnsDefination<YearlyFinancialDataItem>(null),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ── PopulateDropdownsAsync ────────────────────────────────────────
        private async Task PopulateDropdownsAsync(YearlyFinancialDataViewModel model)
        {
            ApiResponseDto<List<ProjectListMilestoneDto>> projectResult =
                await _projectListService.GetAllProjectsForMilestoneAsync();

            if (projectResult.Success && projectResult.Data != null)
            {
                model.ProjectList = projectResult.Data
                    .Where(p => !string.IsNullOrWhiteSpace(p.Parentproject))
                    .OrderBy(p => p.Parentproject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.Parentproject,
                        Text = p.Parentproject
                    })
                    .ToList();
            }
        }

        private async Task<double> GetRequiredDoubleSettingAsync(string id)
        {
            ApiResponseDto<string> setting = await _service.GetSettingValueByIdAsync(id);
            if (!setting.Success || string.IsNullOrWhiteSpace(setting.Data))
            {
                throw new InvalidOperationException($"Required PIMS setting '{id}' is missing.");
            }

            if (!double.TryParse(setting.Data, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                throw new InvalidOperationException($"Required PIMS setting '{id}' has invalid numeric value '{setting.Data}'.");
            }

            if (value <= 0)
            {
                throw new InvalidOperationException($"Required PIMS setting '{id}' must be greater than zero.");
            }

            return value;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetProjectDates(string project)
        {
            if (string.IsNullOrWhiteSpace(project))
                return Json(new { success = false, startDate = string.Empty, endDate = string.Empty });

            ApiResponseDto<ProjectDetailDto> detailResult =
                await _projectDetailsService.GetPimsDetailAsync(project);

            if (!detailResult.Success || detailResult.Data is null)
                return Json(new { success = false, startDate = string.Empty, endDate = string.Empty });

            ProjectDetailDto detail = detailResult.Data;
            string startDate = detail.StartDate.HasValue
                ? detail.StartDate.Value.ToString("dd/MM/yyyy")
                : string.Empty;
            string endDate = detail.RevisedEndDate.HasValue
                ? detail.RevisedEndDate.Value.ToString("dd/MM/yyyy")
                : (detail.EndDate.HasValue
                    ? detail.EndDate.Value.ToString("dd/MM/yyyy")
                    : string.Empty);

            return Json(new { success = true, startDate, endDate });
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────

        
        [HttpPost]
        public async Task<IActionResult> LoadYearlyFinancialDataGrid(
            PaginationFilter<string> request, string? project = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors  = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<YearlyFinancialDataItem> gridConfig =
                await BuildYearlyFinancialDataGridAsync(request, project);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<YearlyFinancialDataItem>> BuildYearlyFinancialDataGridAsync(
            PaginationFilter<string> request, string? project)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            
            string resolvedProject = project
                ?? (filterDict.TryGetValue("project", out string? fp) ? fp : null)
                ?? string.Empty;

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            queryParameters.Page = -1;

            ApiResponseDto<List<YearlyFinancialDataDto>> pagedData =
                await _service.GetAllAsync(resolvedProject, queryParameters);

            List<YearlyFinancialDataItem> items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<YearlyFinancialDataItem>>(pagedData.Data)
                : [];

            PaginationModel pagination = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<YearlyFinancialDataItem>
            {
                GridId             = "costCenterListGrid",
                Title              = "Yearly Financial Details",
                ShowCheckboxColumn = false,
                ShowPagination     = false,
                KeyProperty        = "Year",
                AllowAdd           = true,
                AddFunction        = "addYearlyFinancialData",
                AllowEdit          = true,
                EditFunction       = "editYearlyFinancialData",
                AllowDelete        = true,
                DeleteFunction     = "deleteYearlyFinancialData",
                AllowView          = false,
                ViewFunction       = "viewYearlyFinancialData",
                ExtraFilterMethod  = "getYearlyFinancialDataExtraFilters",
                BindGridUrl        = "/PIMS/YearlyFinancialData/LoadYearlyFinancialDataGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<YearlyFinancialDataItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────

        private void SetCurrentCostingUser()
        {
            ViewBag.CurrentCostingUser = User?.Identity?.Name ?? string.Empty;
        }

        
        [HttpGet]
        public IActionResult Create(string? project)
        {
            SetCurrentCostingUser();

            return PartialView("_AddEditYearlyFinancialData", new YearlyFinancialDataItem
            {
                Project = project ?? string.Empty
            });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] YearlyFinancialDataDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            
            if (!string.IsNullOrWhiteSpace(dto.CostedBy))
            {
                int atIndex = dto.CostedBy.IndexOf('@');
                if (atIndex > 0)
                {
                    dto.CostedBy = dto.CostedBy.Substring(0, atIndex);
                }
            }

            ApiResponseDto<YearlyFinancialDataDto> result = await _service.CreateAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record created successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(short year, string project)
        {
            ApiResponseDto<YearlyFinancialDataDto> result = await _service.GetByKeyAsync(year, project);
            if (!result.Success || result.Data is null)
                return NotFound($"Yearly financial record for project '{project}' year {year} not found.");

            SetCurrentCostingUser();

            YearlyFinancialDataItem item = _mapper.Map<YearlyFinancialDataItem>(result.Data);
            return PartialView("_AddEditYearlyFinancialData", item);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short year, string project, [FromBody] YearlyFinancialDataDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            
            if (!string.IsNullOrWhiteSpace(dto.CostedBy))
            {
                int atIndex = dto.CostedBy.IndexOf('@');
                if (atIndex > 0)
                {
                    dto.CostedBy = dto.CostedBy.Substring(0, atIndex);
                }
            }

            ApiResponseDto<YearlyFinancialDataDto> result = await _service.UpdateAsync(year, project, dto);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        
        [HttpDelete]
        public async Task<IActionResult> Delete(short year, string project)
        {
            ApiResponseDto<object> result = await _service.DeleteAsync(year, project);
            return result.Success
                ? Json(new { success = true, message = "Yearly financial record deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        
        [HttpGet]
        public async Task<IActionResult> GetPactCosts(string project, short year)
        {
            ApiResponseDto<PactProjectYearCostsDto> result =
                await _service.GetPactCostsAsync(project, year);

            if (!result.Success || result.Data is null)
                return Json(new { success = false, message = "Failed to load PACT costs" });

            PactCostsItem item = _mapper.Map<PactCostsItem>(result.Data);
            return Json(new { success = true, data = item });
        }
    }
}
