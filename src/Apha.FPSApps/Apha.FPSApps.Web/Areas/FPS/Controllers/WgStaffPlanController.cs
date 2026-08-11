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
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WgStaffPlanController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;

        public WgStaffPlanController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IWorkGroupService workGroupService)
        {
            _mapper = mapper;
            _profitCentreService = profitCentreService;
            _workGroupService = workGroupService;
        }

        /// <summary>
        /// Displays the Workgroup Staff Plan page with cascading Resource Centre → Work Group dropdowns.
        /// </summary>
        public async Task<IActionResult> Index(string? resourceCentre = null, string? workGroup = null)
        {
            var resourceCentreList = await GetResourceCentreListAsync();
            var selectedRc = resourceCentreList.Any(r => r.Value == resourceCentre) ? resourceCentre! : string.Empty;

            var workGroupList = string.IsNullOrWhiteSpace(selectedRc)
                ? new List<SelectListItem>()
                : await GetWorkGroupListByResourceCentreAsync(selectedRc);

            var selectedWg = workGroupList.Any(w => w.Value == workGroup) ? workGroup! : string.Empty;

            var grid = await BuildGridAsync(new PaginationFilter<string>(), selectedWg);

            return View(new WgStaffPlanViewModel
            {
                SelectedResourceCentre = selectedRc,
                ResourceCentreList = resourceCentreList,
                SelectedWorkGroup = selectedWg,
                WorkGroupList = workGroupList,
                Grid = grid
            });
        }

        /// <summary>
        /// Returns workgroups for a given resource centre — called client-side via AJAX.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkGroupsByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
                return Json(new { success = false, message = "Resource Centre is required." });

            var response = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(resourceCentre);
            if (!response.Success)
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load work groups." });

            var workGroups = response.Data != null
                ? response.Data.Select(w => w.WorkGroupName).OrderBy(w => w).ToList()
                : new List<string>();

            return Json(new { success = true, data = workGroups });
        }

        /// <summary>
        /// Reloads the grid partial view based on the current workgroup selection.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadGrid(PaginationFilter<string> request, string? workGroup = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildGridAsync(request, workGroup ?? string.Empty);
            return PartialView("_DataGrid", grid);
        }

        private async Task<DataGridConfig<WgStaffPlanViewItem>> BuildGridAsync(
            PaginationFilter<string> request, string workGroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var rows = new List<WgStaffPlanViewItem>();
            PaginationModel pagination = new();

            if (!string.IsNullOrWhiteSpace(workGroup))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _profitCentreService.GetPagedWgStaffPlanAsync(query, workGroup);

                if (response.Success && response.Data != null)
                {
                    rows = _mapper.Map<List<WgStaffPlanViewItem>>(response.Data);

                    if (response.Pagination != null)
                    {
                        pagination.PageNumber   = response.Pagination.PageNumber;
                        pagination.PageSize     = response.Pagination.PageSize;
                        pagination.TotalRecords = response.Pagination.TotalRecords;
                    }
                }
            }

            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<WgStaffPlanViewItem>
            {
                GridId         = "wgStaffPlanGrid",
                KeyProperty    = "StaffId",
                AllowAdd       = false,
                AllowEdit      = false,
                AllowDelete    = false,
                ShowPagination = true,
                BindGridUrl    = "/FPS/WgStaffPlan/LoadGrid",
                ExtraFilterMethod = "getWgStaffPlanExtraFilters",
                Columns        = GridDataProvider.GetColumnsDefination<WgStaffPlanViewItem>(),
                Data           = rows,
                CurrentFilters = filterDict,
                Pagination     = pagination
            };
        }

        private async Task<List<SelectListItem>> GetResourceCentreListAsync()
        {
            var response = await _profitCentreService.GetProfitCentresAsync();
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Where(r => !string.IsNullOrWhiteSpace(r.ProfitCentreId))
                .OrderBy(r => r.ProfitCentreId)
                .Select(r => new SelectListItem(r.ProfitCentreId, r.ProfitCentreId))
                .ToList();
        }

        private async Task<List<SelectListItem>> GetWorkGroupListByResourceCentreAsync(string resourceCentre)
        {
            var response = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(resourceCentre);
            if (!response.Success || response.Data == null)
                return new List<SelectListItem>();

            return response.Data
                .Select(w => w.WorkGroupName)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .OrderBy(w => w)
                .Select(w => new SelectListItem(w, w))
                .ToList();
        }
    }
}
