using Apha.FPSApps.Application.Dtos.FPS;
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
    public class ProjectProfitabilityVlaController : Controller
    {
        //   all rows for summary aggregation. Replaces int.MaxValue to prevent unbounded
        //   memory allocation and excessive DB load. Increase if VLA dataset exceeds this limit.
        private const int SummaryMaxPageSize = 5000;

        private readonly IMapper _mapper;

        private readonly IProjectService _projectService;

        private readonly IProgramService _programService;

        public ProjectProfitabilityVlaController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
        }

        /// <summary>
        /// GET /FPS/ProjectProfitabilityVla — renders the VLA profitability page.
        /// Builds explicit DataGridConfig and populates all 4 filter dropdowns.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var viewModel = new ProjectProfitabilityVlaViewModel();
            await PopulateDropdownsAsync(viewModel);

            //   AllowAdd/Edit/Delete = false (JS showAddButton:false; no edit/delete buttons).
            //   KeyProperty = "Id" — hidden row discriminator; Id is not a visible grid column.
            viewModel.ProfitabilityVlaGrid = new DataGridConfig<ProjectProfitabilityVlaItem>
            {
                GridId             = "projectProfitabilityVlaGrid",
                Title              = "Project Profitability for VLA",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                //   DataGrid AJAX reload; implemented in the Razor view (Phase 12).
                ExtraFilterMethod  = "getProjectProfitabilityVlaExtraFilters",
                BindGridUrl        = "/FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid",
                Data               = new List<ProjectProfitabilityVlaItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<ProjectProfitabilityVlaItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid
        /// AJAX DataGrid reload endpoint — called by the _DataGrid gridManager.
        /// Four optional filter params are merged in by getProjectProfitabilityVlaExtraFilters().
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadProjectProfitabilityVlaGrid(
            PaginationFilter<string> request,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetProjectProfitabilityVlaGridConfigAsync(
                request, projectStatus, programNo, manager, customer);

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// GET /FPS/ProjectProfitabilityVla/GetProjectProfitabilityVlaSummary
        /// Returns JSON summary totals for the 9 ppf-total-* readonly inputs in the HTML prototype.
        /// Called by the Razor view after the grid reloads to update the summary bar.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProjectProfitabilityVlaSummary(
            [FromQuery] string? projectStatus = null,
            [FromQuery] string? programNo = null,
            [FromQuery] string? manager = null,
            [FromQuery] string? customer = null)
        {
            //   mirrors projectprofitability_vla.js updateSummary behaviour.
            //   int.MaxValue to bound memory allocation; see class-level constant declaration.
            var query = new QueryParameters<string> { Page = 1, PageSize = SummaryMaxPageSize };

            var response = await _projectService.GetProjectProfitabilityVlaAsync(
                query,
                projectStatus: string.IsNullOrWhiteSpace(projectStatus) ? null : projectStatus,
                programNo: string.IsNullOrWhiteSpace(programNo) ? null : programNo,
                manager: string.IsNullOrWhiteSpace(manager) ? null : manager,
                customer: string.IsNullOrWhiteSpace(customer) ? null : customer);

            if (!response.Success)
                return StatusCode(500, response.Errors);

            var items = response.Data ?? new List<ProjectProfitabilityVlaDto>();

            return Ok(new
            {
                totalStaffCosts      = items.Sum(i => i.StaffCosts),
                totalTestCost        = items.Sum(i => i.TestCost),
                totalAnimalCosts     = items.Sum(i => i.AnimalCosts),
                totalAdditionalCosts = items.Sum(i => i.AdditionalCosts),
                totalTotalCosts      = items.Sum(i => i.TotalCosts),
                totalBudget          = items.Sum(i => i.Budget ?? 0m),
                totalProfit          = items.Sum(i => i.Profit),
                totalTargetProfit    = items.Sum(i => i.TargetProfit),
                totalOffTarget       = items.Sum(i => i.OffTarget)
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<DataGridConfig<ProjectProfitabilityVlaItem>> GetProjectProfitabilityVlaGridConfigAsync(
            PaginationFilter<string> request,
            string? projectStatus,
            string? programNo,
            string? manager,
            string? customer)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            //   via IProjectService; all 4 filter params optional — no placeholder defaults.
            var response = await _projectService.GetProjectProfitabilityVlaAsync(
                queryParameters,
                projectStatus: string.IsNullOrWhiteSpace(projectStatus) ? null : projectStatus,
                programNo: string.IsNullOrWhiteSpace(programNo) ? null : programNo,
                manager: string.IsNullOrWhiteSpace(manager) ? null : manager,
                customer: string.IsNullOrWhiteSpace(customer) ? null : customer);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProjectProfitabilityVlaItem>>(response.Data.ToList())
                : new List<ProjectProfitabilityVlaItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectProfitabilityVlaItem>
            {
                GridId             = "projectProfitabilityVlaGrid",
                Title              = "Project Profitability for VLA",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                ExtraFilterMethod  = "getProjectProfitabilityVlaExtraFilters",
                BindGridUrl        = "/FPS/ProjectProfitabilityVla/LoadProjectProfitabilityVlaGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectProfitabilityVlaItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        private async Task PopulateDropdownsAsync(ProjectProfitabilityVlaViewModel model)
        {
            //   options: Approved, Completed, Not Approved.
            model.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "",             Text = "All statuses" },
                new SelectListItem { Value = "Approved",     Text = "Approved" },
                new SelectListItem { Value = "Completed",    Text = "Completed" },
                new SelectListItem { Value = "Not Approved", Text = "Not Approved" }
            };

            //   separate from CRUD resource per layer boundary rule.
            var programResult = await _programService.GetAllProgramsAsync();
            if (programResult.Success && programResult.Data != null)
            {
                model.ProgramList = programResult.Data
                    .OrderBy(p => p.ProgramNo)
                    .Select(p => new SelectListItem
                    {
                        Value    = p.ProgramNo,
                        Text     = string.IsNullOrWhiteSpace(p.ProgramName)
                                      ? p.ProgramNo
                                      : $"{p.ProgramNo} — {p.ProgramName}",
                        Selected = string.Equals(model.SelectedProgram, p.ProgramNo,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            //   (existing /api/v1/employee lookup); ManagerDto.Name used as both Value and Text.
            var managerResult = await _projectService.GetManagersAsync();
            if (managerResult.Success && managerResult.Data != null)
            {
                model.ManagerList = managerResult.Data
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                    .OrderBy(m => m.Name)
                    .Select(m => new SelectListItem
                    {
                        Value    = m.Name,
                        Text     = m.Name,
                        Selected = string.Equals(model.SelectedManager, m.Name,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }

            //   (existing /api/v1/customer lookup); CustomerDto.Customer used as both Value and Text.
            var customerResult = await _projectService.GetAllCustomersAsync();
            if (customerResult.Success && customerResult.Data != null)
            {
                model.CustomerList = customerResult.Data
                    .Where(c => !string.IsNullOrWhiteSpace(c.Customer))
                    .OrderBy(c => c.Customer)
                    .Select(c => new SelectListItem
                    {
                        Value    = c.Customer,
                        Text     = c.Customer,
                        Selected = string.Equals(model.SelectedCustomer, c.Customer,
                                      StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }
    }
}
