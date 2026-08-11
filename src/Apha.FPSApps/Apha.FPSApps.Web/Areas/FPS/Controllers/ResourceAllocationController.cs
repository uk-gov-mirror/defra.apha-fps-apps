using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    /// <summary>
    /// MVC controller for Stage 2 Check Resource Allocation (frmResourceAllocation).
    /// Read-only view showing staff allocation and jobs grids for a selected workgroup grade.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ResourceAllocationController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IResourceAllocationService _resourceAllocationService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;
        private readonly IWorkGroupService _workGroupService;

        public ResourceAllocationController(
            IMapper mapper,
            IResourceAllocationService resourceAllocationService,
            IProfitCentreService profitCentreService,
            IWorkGroupGradeService workGroupGradeService,
            IWorkGroupService workGroupService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _resourceAllocationService = resourceAllocationService ?? throw new ArgumentNullException(nameof(resourceAllocationService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
            _workGroupService = workGroupService ?? throw new ArgumentNullException(nameof(workGroupService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? resourceCentre = null, string? workGroup = null)
        {
            var viewModel = new ResourceAllocationViewModel
            {
                ResourceCentres = await PopulateResourceCentresAsync(),
                SelectedResourceCentre = resourceCentre ?? string.Empty,
                SelectedWorkGroup = workGroup ?? string.Empty,
                StaffAllocationGrid = BuildStaffAllocationGridConfig(new List<ResourceStaffAllocationItem>()),
                StaffJobsGrid = BuildStaffJobsGridConfig(new List<ResourceStaffJobItem>())
            };

            // Populate workgroup dropdown if resource centre is selected
            if (!string.IsNullOrWhiteSpace(resourceCentre))
            {
                var workGroupsResponse = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(resourceCentre);
                if (workGroupsResponse.Success && workGroupsResponse.Data != null)
                {
                    viewModel.WorkGroupList = workGroupsResponse.Data
                        .Select(w => new SelectListItem
                        {
                            Value = w.WorkGroupName,
                            Text = w.WorkGroupName
                        })
                        .OrderBy(w => w.Text)
                        .ToList();
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Returns workgroup grades as JSON for a selected resource centre (used by the grade dropdown).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGradesByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
                return Json(new { success = false, message = "Resource Centre is required." });

            var response = await _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(resourceCentre);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });

            var grades = (response.Data ?? [])
                .Select(g => new { value = g.WgGrade, text = g.WgGrade })
                .ToList();

            return Json(new { success = true, data = grades });
        }

        /// <summary>
        /// Loads the staff allocation DataGrid for a given workgroup grade (supports pagination, sorting, filtering).
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffAllocationGrid(PaginationFilter<string> request, [FromForm] string? workGroupGrade)
        {
            if (string.IsNullOrWhiteSpace(workGroupGrade))
                return PartialView("_DataGrid", BuildStaffAllocationGridConfig([]));

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff allocations." });

            var items = (response.Data ?? []).Select(d => _mapper.Map<ResourceStaffAllocationItem>(d)).ToList();
            var pagination = BuildPaginationModel(response.Pagination, request);
            var filters = ParseFilters(request.Filter);

            return PartialView("_DataGrid", BuildStaffAllocationGridConfig(items, pagination, filters));
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffAllocationTotals(string workGroupGrade)
        {
            if (string.IsNullOrWhiteSpace(workGroupGrade))
                return Json(new { success = false, message = "WorkGroup Grade is required." });

            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var response = await _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff allocations." });

            var items = response.Data ?? [];

            double totalHrsAvail = Math.Round(items.Sum(i => i.HrsAvail ?? 0), 2);
            double totalPlannedHours = Math.Round(items.Sum(i => i.PlannedHours), 2);
            double totalAppChargeHours = Math.Round(items.Sum(i => i.AppChargeHours), 2);
            double totalChargeHours = Math.Round(items.Sum(i => i.ChargeHours), 2);

            string allocationPct = totalHrsAvail == 0 ? "" : FormatPct(totalPlannedHours / totalHrsAvail);
            string assuredUtilPct = totalHrsAvail == 0 ? "" : FormatPct(totalAppChargeHours / totalHrsAvail);
            string totalUtilPct = totalHrsAvail == 0 ? "" : FormatPct(totalChargeHours / totalHrsAvail);

            return Json(new
            {
                success = true,
                hrsAvail = totalHrsAvail,
                plannedHrs = totalPlannedHours,
                allocationPct,
                assuredChargeHrs = totalAppChargeHours,
                assuredUtilPct,
                totalChargeHrs = totalChargeHours,
                totalUtilPct
            });
        }

        private static string FormatPct(double value) => (value * 100).ToString("0.##") + "%";

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffJobsGrid(PaginationFilter<string> request, [FromForm] string? staffId)
        {
            if (string.IsNullOrWhiteSpace(staffId))
                return PartialView("_DataGrid", BuildStaffJobsGridConfig([]));

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _resourceAllocationService.GetPagedStaffJobDetailsByStaffIdAsync(staffId, query);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff jobs." });

            var items = (response.Data ?? []).Select(d => _mapper.Map<ResourceStaffJobItem>(d)).ToList();
            var pagination = BuildPaginationModel(response.Pagination, request);
            var filters = ParseFilters(request.Filter);

            return PartialView("_DataGrid", BuildStaffJobsGridConfig(items, pagination, filters));
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private PaginationModel BuildPaginationModel(object? pagination, PaginationFilter<string> request)
        {
            var model = _mapper.Map<PaginationModel>(pagination) ?? new PaginationModel();
            model.SortColumn = request.SortBy;
            model.SortDirection = request.Descending;
            return model;
        }

        private static Dictionary<string, string>? ParseFilters(string? filter) =>
            !string.IsNullOrEmpty(filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                : null;

        private static DataGridConfig<ResourceStaffAllocationItem> BuildStaffAllocationGridConfig(
            List<ResourceStaffAllocationItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "StaffAllocationGrid",
                Title = "",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "OnStaffRowSelect",
                BindGridUrl = "/FPS/ResourceAllocation/LoadStaffAllocationGrid",
                ExtraFilterMethod = "GetStaffAllocationExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffAllocationItem>(),
                ColumnGroups =
                [
                    new() { Label = "", Span = 4 },
                    new() { Label = "Assured Work", Span = 2 },
                    new() { Label = "Total Work", Span = 2 },
                ],
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private static DataGridConfig<ResourceStaffJobItem> BuildStaffJobsGridConfig(
            List<ResourceStaffJobItem> data,
            PaginationModel? pagination = null,
            Dictionary<string, string>? filters = null) =>
            new()
            {
                GridId = "StaffJobsGrid",
                Title = "Jobs for Staff",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffId",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = false,
                BindGridUrl = "/FPS/ResourceAllocation/LoadStaffJobsGrid",
                ExtraFilterMethod = "GetStaffJobsExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<ResourceStaffJobItem>(),
                Pagination = pagination ?? new PaginationModel(),
                CurrentFilters = filters ?? new Dictionary<string, string>()
            };

        private async Task<List<SelectListItem>> PopulateResourceCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            return result.Success && result.Data != null
                ? result.Data.Select(p => new SelectListItem { Value = p.ProfitCentreId, Text = p.ProfitCentreId }).ToList()
                : new List<SelectListItem>();
        }
    }
}
