using Apha.FPSApps.Application.Dtos;
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
using System.Globalization;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class CostCentreMaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        // Correct chain: Controller → ICostCentreService → IFpsApiClient → IFpsCostCentreApiClient → HTTP
        private readonly ICostCentreService _costCentreService;

        public CostCentreMaintenanceController(IMapper mapper, ICostCentreService costCentreService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _costCentreService = costCentreService ?? throw new ArgumentNullException(nameof(costCentreService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new CostCentreMaintenanceViewModel();

            // an empty grid with default Add button regardless of JS-derived operations profile.
            // AllowAdd:true from JS showAddButton:true; AllowEdit+AllowDelete:true from JS
            // actions column containing both edit and delete buttons (costcenter_maintenance.js).
            var defaultRequest = new PaginationFilter<string>
            {
                Filter    = "{}",
                SortBy    = "CostCentreNo",
                Descending = false
            };
            viewModel.CostCentreGrid = await GetCostCentreGridConfigAsync(defaultRequest);

            await PopulateDropdownsAsync(viewModel);

            return View(viewModel);
        }

        // ── Dropdown Population ───────────────────────────────────────────────

        // (returns CostCentreWorkgroupDto), NOT the CRUD paged method; keeps CRUD vs lookup separation.
        // Populates the Index ViewModel's ProfitCentreList property (used by Index view).
        private async Task PopulateDropdownsAsync(CostCentreMaintenanceViewModel model)
        {
            var lookupResult = await _costCentreService.GetAllCostCentresAsync();
            if (lookupResult.Success && lookupResult.Data != null)
            {
                // CostCentreWorkgroupDto.ProfitCentre → SelectListItem.Value and .Text
                model.ProfitCentreList = lookupResult.Data
                    .Where(item => !string.IsNullOrWhiteSpace(item.ProfitCentre))
                    .Select(item => item.ProfitCentre!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p)
                    .Select(p => new SelectListItem { Value = p, Text = p })
                    .ToList();
            }
        }

        // directly so that Create GET and Edit GET return the _AddEditCostCentre partial with a populated
        // ProfitCentre dropdown.  Without this call the dropdown was always empty in both Add and Edit modes.
        private async Task PopulatePartialDropdownsAsync()
        {
            var lookupResult = await _costCentreService.GetAllCostCentresAsync();
            var items = new List<SelectListItem>();
            if (lookupResult.Success && lookupResult.Data != null)
            {
                items = lookupResult.Data
                    .Where(item => !string.IsNullOrWhiteSpace(item.ProfitCentre))
                    .Select(item => item.ProfitCentre!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p)
                    .Select(p => new SelectListItem { Value = p, Text = p })
                    .ToList();
            }
            ViewBag.ProfitCentreList = items;
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadCostCentreGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Invalid request data",
                    errors   = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCostCentreGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<CostCentreItem>> GetCostCentreGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedData = await _costCentreService.GetAllCostCentresPagedAsync(queryParameters);

            var items = new List<CostCentreItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<CostCentreItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<CostCentreItem>
            {
                GridId             = "costcenterGrid",
                Title              = "Cost Centres Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "CostCentreNo",
                AllowAdd           = true,
                AddFunction        = "addCostCentre",
                AllowEdit          = true,
                EditFunction       = "editCostCentre",
                AllowDelete        = true,
                DeleteFunction     = "deleteCostCentre",
                BindGridUrl        = "/FPS/CostCentreMaintenance/LoadCostCentreGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<CostCentreItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD — Create ─────────────────────────────────────────────────────

        // Phase 14 fix: await PopulatePartialDropdownsAsync() so ViewBag.ProfitCentreList is set
        // before the partial is rendered; previously the ProfitCentre dropdown was always empty.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulatePartialDropdownsAsync();
            return PartialView("_AddEditCostCentre", new CostCentreItem());
        }

        // No [ValidateAntiForgeryToken] — endpoint receives JSON body, not form post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CostCentreDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Please correct the errors below.",
                    errors   = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field   = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var result = await _costCentreService.CreateCostCentreAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Cost Centre created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create cost centre.";
            return Json(new
            {
                success = false,
                message  = errorMessage,
                errors   = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field   = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Edit ───────────────────────────────────────────────────────

        // id supplied as query string; culture-invariant parse prevents decimal separator issues
        // Phase 14 fix: await PopulatePartialDropdownsAsync() so ViewBag.ProfitCentreList is set
        // before the partial is rendered; previously the ProfitCentre dropdown was always empty.
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Cost Centre number is required" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            var result = await _costCentreService.GetCostCentreByIdAsync(costCentreNo);

            if (result.Success && result.Data != null)
            {
                await PopulatePartialDropdownsAsync();
                var item = _mapper.Map<CostCentreItem>(result.Data);
                return PartialView("_AddEditCostCentre", item);
            }

            return Json(new { success = false, message = $"Cost Centre '{id}' not found." });
        }

        // ICostCentreService.UpdateCostCentreAsync(double costCentreNo, CostCentreDto dto) signature
        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] CostCentreDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message  = "Please correct the errors below.",
                    errors   = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field   = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // dto.CostCentreNo may differ if user edits the number (matches UpdateCostCentreAsync signature)
            var result = await _costCentreService.UpdateCostCentreAsync(costCentreNo, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Cost Centre updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update cost centre.";
            return Json(new
            {
                success = false,
                message  = errorMessage,
                errors   = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field   = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Delete ─────────────────────────────────────────────────────

        // id supplied as query string from DataGrid delete button; culture-invariant parse
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Cost Centre number is required" });
            }

            if (!double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out var costCentreNo))
            {
                return Json(new { success = false, message = $"Invalid Cost Centre number: '{id}'" });
            }

            var result = await _costCentreService.DeleteCostCentreAsync(costCentreNo);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Cost Centre deleted successfully" });
            }

            var firstError = result.Errors?.FirstOrDefault();
            var errorMessage = firstError?.Code == "DB_POSTGRES_ERROR"
                ? "This cost centre cannot be deleted because it is referenced by other records."
                : firstError?.Message ?? "Unable to delete the cost centre as it may be in use.";

            return Json(new { success = false, message = errorMessage });
        }
    }
}
