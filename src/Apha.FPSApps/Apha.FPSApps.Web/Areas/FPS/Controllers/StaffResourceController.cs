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
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class StaffResourceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IStaffJobService _staffJobService;

        public StaffResourceController(
            IMapper mapper,
            IProfitCentreService profitCentreService,
            IWorkGroupService workGroupService,
            IStaffJobService staffJobService)
        {
            _mapper = mapper;
            _profitCentreService = profitCentreService;
            _workGroupService = workGroupService;
            _staffJobService = staffJobService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? profitCentre = null)
        {
            var viewModel = new StaffResourceViewModel
            {
                SelectedProfitCentre = profitCentre ?? string.Empty
            };

            await PopulateProfitCentresAsync(viewModel);

            // Auto-select the first profit centre when none is specified
            if (string.IsNullOrWhiteSpace(viewModel.SelectedProfitCentre) && viewModel.ProfitCentreList.Count > 0)
            {
                var first = viewModel.ProfitCentreList.First().Value ?? string.Empty;
                viewModel.SelectedProfitCentre = first;
                foreach (var item in viewModel.ProfitCentreList)
                    item.Selected = string.Equals(item.Value, first, StringComparison.OrdinalIgnoreCase);
            }

            viewModel.WorkgroupGrid = await GetWorkgroupGridConfigAsync(new QueryParameters<string>(), null, viewModel.SelectedProfitCentre);
            viewModel.StaffGrid = GetStaffGridConfig();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadWorkgroupGrid(
            PaginationFilter<string> request, string? profitCentre = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var gridConfig = await GetWorkgroupGridConfigAsync(queryParameters, filterDict, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string? workgroup = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var gridConfig = await GetStaffGridConfigAsync(queryParameters, filterDict, workgroup);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<StaffResourceWorkgroupItem>> GetWorkgroupGridConfigAsync(
            QueryParameters<string> queryParameters, Dictionary<string, string>? filterDict, string? profitCentre)
        {
            var items = new List<StaffResourceWorkgroupItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var response = await _workGroupService.GetWorkGroupsByProfitCentreAsync(queryParameters, profitCentre);
                if (response.Success && response.Data != null)
                {
                    items = response.Data.Select(d => new StaffResourceWorkgroupItem
                    {
                        WorkGroupName = d.WorkGroupName
                    }).ToList();

                    paginationModel = response.Pagination == null
                        ? new PaginationModel()
                        : _mapper.Map<PaginationModel>(response.Pagination);
                }
            }

            paginationModel.SortColumn = queryParameters.SortBy;
            paginationModel.SortDirection = queryParameters.Descending;

            return new DataGridConfig<StaffResourceWorkgroupItem>
            {
                GridId = "ruvWorkgroupGrid",
                Title = "Workgroup",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                KeyProperty = "WorkGroupName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                ExtraFilterMethod = "getRuvWorkgroupExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadWorkgroupGrid), "StaffResource", new { area = "FPS" })!,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceWorkgroupItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private DataGridConfig<StaffResourceStaffItem> GetStaffGridConfig()
        {
            return new DataGridConfig<StaffResourceStaffItem>
            {
                GridId = "ruvStaffGrid",
                Title = "Staff Resource Utilisation",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getRuvStaffExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadStaffGrid), "StaffResource", new { area = "FPS" })!,
                Data = new List<StaffResourceStaffItem>(),
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceStaffItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private async Task<DataGridConfig<StaffResourceStaffItem>> GetStaffGridConfigAsync(
            QueryParameters<string> queryParameters, Dictionary<string, string>? filterDict, string? workgroup)
        {
            var items = new List<StaffResourceStaffItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(workgroup))
            {
                var response = await _staffJobService.GetStaffResourceUtilisationAsync(queryParameters, workgroup);
                if (response.Success && response.Data != null)
                {
                    items = response.Data.Select(d => new StaffResourceStaffItem
                    {
                        WgGrade = d.WgGrade,
                        Name = d.Name,
                        TotalH = d.HrsAvail,
                        Ztw = d.PlannedZt,
                        Avail = d.AvailSoct,
                        Left = d.Left,
                        ApprovedPlan = d.ApprovedSoct,
                        ApprovedUtil = d.ApprovedUtilPct,
                        NotApprovedPlan = d.NotApprovedSoct,
                        NotApprovedUtil = d.NotApprovedUtilPct,
                        TotalPlan = d.ApprovedSoct + d.NotApprovedSoct,
                        TotalUtil = d.TotalUtilPct
                    }).ToList();

                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
                }
            }

            paginationModel.SortColumn = queryParameters.SortBy;
            paginationModel.SortDirection = queryParameters.Descending;

            return new DataGridConfig<StaffResourceStaffItem>
            {
                GridId = "ruvStaffGrid",
                Title = "Staff Resource Utilisation",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffName",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getRuvStaffExtraFilters",
                BindGridUrl = Url.Action(nameof(LoadStaffGrid), "StaffResource", new { area = "FPS" })!,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StaffResourceStaffItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task PopulateProfitCentresAsync(StaffResourceViewModel model)
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            model.ProfitCentreList = result.Data == null
                ? new List<SelectListItem>()
                : result.Data.Select(p => new SelectListItem
                {
                    Value = p.ProfitCentreId,
                    Text = p.ProfitCentreId,
                    Selected = string.Equals(model.SelectedProfitCentre, p.ProfitCentreId, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }
    }
}
